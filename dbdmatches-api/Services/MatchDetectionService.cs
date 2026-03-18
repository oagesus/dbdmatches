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

        var killerLoadoutDelta = (int)deltas.GetValueOrDefault("DBD_SlasherFullLoadout");
        var survivorLoadoutDelta = (int)deltas.GetValueOrDefault("DBD_CamperFullLoadout");

        var killerSkullsChanged = existingSnapshots.TryGetValue("DBD_KillerSkulls", out var killerSkullsSnapshot)
            && currentStats.TryGetValue("DBD_KillerSkulls", out var killerSkullsCurrent)
            && Math.Abs(killerSkullsCurrent - killerSkullsSnapshot.StatValue) > 0;
        var survivorSkullsChanged = existingSnapshots.TryGetValue("DBD_CamperSkulls", out var survivorSkullsSnapshot)
            && currentStats.TryGetValue("DBD_CamperSkulls", out var survivorSkullsCurrent)
            && Math.Abs(survivorSkullsCurrent - survivorSkullsSnapshot.StatValue) > 0;

        var nonLoadoutKillerMatch = killerSkullsChanged && killerLoadoutDelta == 0;
        var nonLoadoutSurvivorMatch = survivorSkullsChanged && survivorLoadoutDelta == 0;

        if (nonLoadoutKillerMatch || nonLoadoutSurvivorMatch)
        {
            logger.LogInformation("Non-loadout match detected for user {UserId} (killerSkullsChanged={KillerChanged}, survivorSkullsChanged={SurvivorChanged}). Consuming stats.",
                userId, killerSkullsChanged, survivorSkullsChanged);
        }
        else if (killerLoadoutDelta > 1 || survivorLoadoutDelta > 1 || (killerLoadoutDelta > 0 && survivorLoadoutDelta > 0))
        {
            logger.LogWarning("Multiple matches detected between polls for user {UserId} (killer={KillerDelta}, survivor={SurvivorDelta}). Skipping match creation.",
                userId, killerLoadoutDelta, survivorLoadoutDelta);
        }
        else if (killerLoadoutDelta == 1)
        {
            var killerInfo = KillerMappingService.IdentifyKiller(deltas);

            // Cross-killer contamination: check if stats from a different killer are in the deltas
            var detectedKillerStatKeys = new HashSet<string>();
            if (killerInfo != null)
            {
                if (killerInfo.Stat1Key != null) detectedKillerStatKeys.Add(killerInfo.Stat1Key);
                if (killerInfo.Stat2Key != null) detectedKillerStatKeys.Add(killerInfo.Stat2Key);
                if (killerInfo.Stat3Key != null) detectedKillerStatKeys.Add(killerInfo.Stat3Key);
            }
            var hasOtherKillerActivity = deltas.Keys
                .Any(k => KillerMappingService.AllKillerStatKeys.Contains(k) && !detectedKillerStatKeys.Contains(k));

            var rawSacrifices = (int)deltas.GetValueOrDefault("DBD_SacrificedCampers");
            var rawKills = (int)deltas.GetValueOrDefault("DBD_KilledCampers");
            var exceedsMaxEliminations = rawSacrifices + rawKills > 4;

            var isContaminated = hasOtherKillerActivity || exceedsMaxEliminations;

            var sacrificesDelta = isContaminated ? 0 : rawSacrifices;
            var killsDelta = isContaminated ? 0 : rawKills;

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
                PowerStat1 = 0,
                PowerStat2 = 0,
                PowerStat3 = 0,
                BloodpointsEarned = 0,
                Result = result,
                IsContaminated = isContaminated,
                PlayedAt = DateTimeOffset.UtcNow
            };

            db.MatchKillers.Add(match);
            if (!isContaminated)
                await UpdateStreaks(db, userId, result, isKiller: true, killerName: match.Killer);
            logger.LogInformation("Detected killer match for user {UserId}: {Killer}, {Eliminations} eliminations, {Result}{Contaminated}",
                userId, match.Killer, totalEliminations, result,
                isContaminated ? $" (contaminated{(hasOtherKillerActivity ? " - other killer stats" : "")}{(exceedsMaxEliminations ? $" - {rawSacrifices + rawKills} eliminations exceeds max" : "")})" : "");
        }
        else if (survivorLoadoutDelta == 1)
        {
            var escapeDelta = (int)deltas.GetValueOrDefault("DBD_Escape");
            var hatchDelta = (int)deltas.GetValueOrDefault("DBD_EscapeThroughHatch");
            var escaped = escapeDelta > 0;
            var hatchEscape = hatchDelta > 0;

            var match = new MatchSurvivor
            {
                UserId = userId,
                Escaped = escaped,
                HatchEscape = hatchEscape,
                GeneratorsCompleted = 0,
                BloodpointsEarned = 0,
                Result = escaped ? MatchResult.Win : MatchResult.Loss,
                IsContaminated = false,
                PlayedAt = DateTimeOffset.UtcNow
            };

            db.MatchSurvivors.Add(match);
            await UpdateStreaks(db, userId, match.Result, isKiller: false, killerName: null);
            logger.LogInformation("Detected survivor match for user {UserId}: escaped={Escaped}, {Result}",
                userId, escaped, match.Result);
        }

        var matchCreated = killerLoadoutDelta == 1 || survivorLoadoutDelta == 1;
        var nonLoadoutConsumed = nonLoadoutKillerMatch || nonLoadoutSurvivorMatch;

        HashSet<string> deferredStats = [
            "DBD_SacrificedCampers",
            "DBD_KilledCampers",
            "DBD_SlasherFullLoadout",
            "DBD_CamperFullLoadout",
            "DBD_KillerSkulls",
            "DBD_CamperSkulls"
        ];

        var now = DateTimeOffset.UtcNow;
        foreach (var (statName, currentValue) in currentStats)
        {
            if (!KillerMappingService.AllTrackedStatKeys.Contains(statName))
                continue;

            if (!matchCreated && !nonLoadoutConsumed && deferredStats.Contains(statName))
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
