using System.Text.Json;
using DbdMatches.Api.Data;
using DbdMatches.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DbdMatches.Api.Services;

public class MatchDetectionService(
    IConfiguration configuration,
    HttpClient httpClient,
    ILogger<MatchDetectionService> logger)
{
    private const string SteamApiUrl = "https://api.steampowered.com";
    private const int DbdAppId = 381210;

    public async Task<Dictionary<string, double>?> FetchPlayerStats(string steamId)
    {
        var apiKey = configuration["Steam:ApiKey"]
            ?? throw new InvalidOperationException("Steam API key not configured");

        var url = $"{SteamApiUrl}/ISteamUserStats/GetUserStatsForGame/v0002/?appid={DbdAppId}&key={apiKey}&steamid={steamId}";
        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("playerstats", out var playerStats) ||
            !playerStats.TryGetProperty("stats", out var stats))
            return null;

        var result = new Dictionary<string, double>();
        foreach (var stat in stats.EnumerateArray())
        {
            var name = stat.GetProperty("name").GetString()!;
            var value = stat.GetProperty("value").GetDouble();
            result[name] = value;
        }

        return result;
    }

    public async Task SaveInitialSnapshot(AppDbContext db, int userId, string steamId)
    {
        var stats = await FetchPlayerStats(steamId);
        if (stats == null)
        {
            logger.LogWarning("Could not fetch initial stats for {SteamId}", steamId);
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var snapshots = stats
            .Where(s => KillerMappingService.AllTrackedStatKeys.Contains(s.Key))
            .Select(s => new PlayerStatsSnapshot
            {
                UserId = userId,
                StatName = s.Key,
                StatValue = s.Value,
                FetchedAt = now
            })
            .ToList();

        db.PlayerStatsSnapshots.AddRange(snapshots);
        await db.SaveChangesAsync();

        logger.LogInformation("Saved initial snapshot with {Count} stats for user {UserId}", snapshots.Count, userId);
    }

    public async Task DetectMatch(AppDbContext db, int userId, string steamId)
    {
        var currentStats = await FetchPlayerStats(steamId);
        if (currentStats == null)
            return;

        var existingSnapshots = await db.PlayerStatsSnapshots
            .Where(s => s.UserId == userId)
            .ToDictionaryAsync(s => s.StatName, s => s);

        if (existingSnapshots.Count == 0)
        {
            await SaveInitialSnapshot(db, userId, steamId);
            return;
        }

        var deltas = new Dictionary<string, double>();
        foreach (var (statName, currentValue) in currentStats)
        {
            if (!KillerMappingService.AllTrackedStatKeys.Contains(statName))
                continue;

            if (existingSnapshots.TryGetValue(statName, out var snapshot))
            {
                var delta = currentValue - snapshot.StatValue;
                if (delta > 0)
                    deltas[statName] = delta;
            }
            else
            {
                deltas[statName] = currentValue;
            }
        }

        if (deltas.Count == 0)
            return;

        var bpDelta = deltas.GetValueOrDefault("DBD_BloodwebPoints");
        if (bpDelta <= 0)
            return;

        var killerInfo = KillerMappingService.IdentifyKiller(deltas);
        var sacrificesDelta = (int)deltas.GetValueOrDefault("DBD_SacrificedCampers");
        var killsDelta = (int)deltas.GetValueOrDefault("DBD_KilledCampers");
        var escapeDelta = (int)deltas.GetValueOrDefault("DBD_Escape");
        var hatchDelta = (int)deltas.GetValueOrDefault("DBD_EscapeThroughHatch");

        var isKillerMatch = killerInfo != null || sacrificesDelta > 0 || killsDelta > 0;
        var unhookOrHealDelta = (int)deltas.GetValueOrDefault("DBD_UnhookOrHeal");
        var isSurvivorMatch = escapeDelta > 0 || hatchDelta > 0 ||
            deltas.GetValueOrDefault("DBD_GeneratorPct_float") > 0 || unhookOrHealDelta > 0;

        if (isKillerMatch && !isSurvivorMatch)
        {
            var totalEliminations = sacrificesDelta + killsDelta;
            var result = totalEliminations switch
            {
                >= 3 => MatchResult.Win,
                2 => MatchResult.Draw,
                _ => MatchResult.Loss
            };

            var match = new MatchKiller
            {
                UserId = userId,
                Killer = killerInfo?.Name ?? "Untracked Killer",
                Sacrifices = sacrificesDelta,
                Kills = killsDelta,
                PowerStat1 = killerInfo != null ? KillerMappingService.GetPowerStat1Delta(killerInfo, deltas) : 0,
                PowerStat2 = killerInfo != null ? KillerMappingService.GetPowerStat2Delta(killerInfo, deltas) : 0,
                PowerStat3 = killerInfo != null ? KillerMappingService.GetPowerStat3Delta(killerInfo, deltas) : 0,
                BloodpointsEarned = (int)bpDelta,
                Result = result,
                PlayedAt = DateTimeOffset.UtcNow
            };

            db.MatchKillers.Add(match);
            await UpdateStreaks(db, userId, result, isKiller: true, killerName: match.Killer);
            logger.LogInformation("Detected killer match for user {UserId}: {Killer}, {Eliminations} eliminations, {Result}",
                userId, match.Killer, totalEliminations, result);
        }
        else if (isSurvivorMatch && !isKillerMatch)
        {
            var escaped = escapeDelta > 0;
            var hatchEscape = hatchDelta > 0;

            var match = new MatchSurvivor
            {
                UserId = userId,
                Escaped = escaped,
                HatchEscape = hatchEscape,
                GeneratorsCompleted = deltas.GetValueOrDefault("DBD_GeneratorPct_float"),
                BloodpointsEarned = (int)bpDelta,
                Result = escaped ? MatchResult.Win : MatchResult.Loss,
                PlayedAt = DateTimeOffset.UtcNow
            };

            db.MatchSurvivors.Add(match);
            await UpdateStreaks(db, userId, match.Result, isKiller: false, killerName: null);
            logger.LogInformation("Detected survivor match for user {UserId}: escaped={Escaped}, {Result}",
                userId, escaped, match.Result);
        }
        else if (isKillerMatch && isSurvivorMatch)
        {
            logger.LogWarning("Ambiguous match detected for user {UserId} - both killer and survivor stats changed. Skipping.", userId);
            // This can happen if polling missed a match boundary.
            // Stats still get updated below so the next poll starts fresh.
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var (statName, currentValue) in currentStats)
        {
            if (!KillerMappingService.AllTrackedStatKeys.Contains(statName))
                continue;

            if (existingSnapshots.TryGetValue(statName, out var snapshot))
            {
                snapshot.StatValue = currentValue;
                snapshot.FetchedAt = now;
            }
            else
            {
                db.PlayerStatsSnapshots.Add(new PlayerStatsSnapshot
                {
                    UserId = userId,
                    StatName = statName,
                    StatValue = currentValue,
                    FetchedAt = now
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task UpdateStreaks(AppDbContext db, int userId, MatchResult result, bool isKiller, string? killerName)
    {
        var streak = await db.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        if (streak == null)
        {
            streak = new Streak { UserId = userId };
            db.Streaks.Add(streak);
        }

        if (result == MatchResult.Win)
        {
            streak.CurrentOverall++;
            if (streak.CurrentOverall > streak.BestOverall)
                streak.BestOverall = streak.CurrentOverall;

            if (isKiller)
            {
                streak.CurrentKiller++;
                if (streak.CurrentKiller > streak.BestKiller)
                    streak.BestKiller = streak.CurrentKiller;
            }
            else
            {
                streak.CurrentSurvivor++;
                if (streak.CurrentSurvivor > streak.BestSurvivor)
                    streak.BestSurvivor = streak.CurrentSurvivor;
            }
        }
        else
        {
            streak.CurrentOverall = 0;

            if (isKiller)
                streak.CurrentKiller = 0;
            else
                streak.CurrentSurvivor = 0;
        }

        if (isKiller && killerName != null)
        {
            var killerStreak = await db.StreakKillers
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Killer == killerName);

            if (killerStreak == null)
            {
                killerStreak = new StreakKiller { UserId = userId, Killer = killerName };
                db.StreakKillers.Add(killerStreak);
            }

            if (result == MatchResult.Win)
            {
                killerStreak.CurrentStreak++;
                if (killerStreak.CurrentStreak > killerStreak.BestStreak)
                    killerStreak.BestStreak = killerStreak.CurrentStreak;
            }
            else
            {
                killerStreak.CurrentStreak = 0;
            }
        }
    }
}
