using DbdMatches.Api.Data;
using DbdMatches.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DbdMatches.Api.Services;

public class LeaderboardService(IServiceProvider serviceProvider, ILogger<LeaderboardService> logger)
{
    private List<LeaderboardEntry> _cachedEntries = [];
    private DateTimeOffset _lastCalculated = DateTimeOffset.MinValue;
    private readonly Lock _lock = new();

    public DateTimeOffset LastCalculated => _lastCalculated;

    public async Task CalculateAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var users = await db.Users
            .Where(u => !u.IsBlocked)
            .Select(u => new { u.Id, u.SteamId, u.DisplayName, u.VanityUrl, u.AvatarUrl })
            .ToListAsync();

        var allKillerMatches = await db.MatchKillers.ToListAsync();
        var allSurvivorMatches = await db.MatchSurvivors.ToListAsync();

        var entries = new List<LeaderboardEntry>();

        foreach (var user in users)
        {
            var killerMatches = allKillerMatches.Where(m => m.UserId == user.Id).OrderBy(m => m.PlayedAt).ToList();
            var survivorMatches = allSurvivorMatches.Where(m => m.UserId == user.Id).OrderBy(m => m.PlayedAt).ToList();

            var allMatches = killerMatches
                .Select(m => new { m.PlayedAt, m.Result, Role = "killer", Killer = (string?)m.Killer })
                .Concat(survivorMatches.Select(m => new { m.PlayedAt, m.Result, Role = "survivor", Killer = (string?)null }))
                .OrderBy(m => m.PlayedAt)
                .ToList();

            if (allMatches.Count == 0) continue;

            var overallBest = CalculateBestStreak(allMatches.Select(m => m.Result));
            var killerBest = CalculateBestStreak(allMatches.Where(m => m.Role == "killer").Select(m => m.Result));
            var survivorBest = CalculateBestStreak(allMatches.Where(m => m.Role == "survivor").Select(m => m.Result));

            var killerNames = allMatches.Where(m => m.Killer != null).Select(m => m.Killer!).Distinct().ToList();
            var perKillerBest = new Dictionary<string, int>();
            foreach (var name in killerNames)
            {
                perKillerBest[name] = CalculateBestStreak(
                    allMatches.Where(m => m.Killer == name).Select(m => m.Result));
            }

            // Time-filtered streaks
            var periods = new Dictionary<string, DateTimeOffset>
            {
                ["30d"] = DateTimeOffset.UtcNow.AddDays(-30),
                ["90d"] = DateTimeOffset.UtcNow.AddDays(-90),
                ["1y"] = DateTimeOffset.UtcNow.AddYears(-1)
            };

            var periodStreaks = new Dictionary<string, PeriodStreak>();
            foreach (var (period, since) in periods)
            {
                var filtered = allMatches.Where(m => m.PlayedAt >= since).ToList();
                periodStreaks[period] = new PeriodStreak
                {
                    Overall = CalculateBestStreak(filtered.Select(m => m.Result)),
                    Killer = CalculateBestStreak(filtered.Where(m => m.Role == "killer").Select(m => m.Result)),
                    Survivor = CalculateBestStreak(filtered.Where(m => m.Role == "survivor").Select(m => m.Result)),
                    PerKiller = killerNames.ToDictionary(
                        name => name,
                        name => CalculateBestStreak(filtered.Where(m => m.Killer == name).Select(m => m.Result)))
                };
            }

            entries.Add(new LeaderboardEntry
            {
                UserId = user.Id,
                SteamId = user.SteamId,
                DisplayName = user.DisplayName,
                VanityUrl = user.VanityUrl,
                AvatarUrl = user.AvatarUrl,
                BestOverall = overallBest,
                BestKiller = killerBest,
                BestSurvivor = survivorBest,
                PerKillerBest = perKillerBest,
                PeriodStreaks = periodStreaks,
                TotalMatches = allMatches.Count
            });
        }

        lock (_lock)
        {
            _cachedEntries = entries;
            _lastCalculated = DateTimeOffset.UtcNow;
        }

        logger.LogInformation("Leaderboard calculated for {Count} users at {Time}", entries.Count, _lastCalculated);
    }

    public LeaderboardResult GetLeaderboard(string role, string? killer, string? period, int page, int pageSize, string? search = null)
    {
        List<LeaderboardEntry> entries;
        lock (_lock)
        {
            entries = _cachedEntries;
        }

        var ranked = entries.Select(e =>
        {
            int streak;
            if (period != null && e.PeriodStreaks.TryGetValue(period, out var ps))
            {
                streak = role switch
                {
                    "killer" when killer != null => ps.PerKiller.GetValueOrDefault(killer),
                    "killer" => ps.Killer,
                    "survivor" => ps.Survivor,
                    _ => ps.Overall
                };
            }
            else
            {
                streak = role switch
                {
                    "killer" when killer != null => e.PerKillerBest.GetValueOrDefault(killer),
                    "killer" => e.BestKiller,
                    "survivor" => e.BestSurvivor,
                    _ => e.BestOverall
                };
            }

            return new { Entry = e, Streak = streak };
        })
        .Where(x => x.Streak > 0)
        .OrderByDescending(x => x.Streak)
        .Select((x, i) => new { x.Entry, x.Streak, Rank = i + 1 })
        .ToList();

        var filtered = ranked;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            if (term.Contains("steamcommunity.com/id/", StringComparison.OrdinalIgnoreCase))
            {
                var vanity = term.Split("/id/", StringSplitOptions.None).LastOrDefault()?.TrimEnd('/') ?? "";
                filtered = ranked.Where(x =>
                    x.Entry.VanityUrl != null && x.Entry.VanityUrl.Equals(vanity, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            else if (term.Contains("steamcommunity.com/profiles/", StringComparison.OrdinalIgnoreCase))
            {
                var steamId = term.Split("/profiles/", StringSplitOptions.None).LastOrDefault()?.TrimEnd('/') ?? "";
                filtered = ranked.Where(x =>
                    x.Entry.SteamId == steamId
                ).ToList();
            }
            else
            {
                filtered = ranked.Where(x =>
                    x.Entry.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Entry.SteamId.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (x.Entry.VanityUrl != null && x.Entry.VanityUrl.Contains(term, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
        }

        var totalCount = filtered.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var paged = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var items = paged.Select(x => new LeaderboardItem
        {
            Rank = x.Rank,
            SteamId = x.Entry.SteamId,
            DisplayName = x.Entry.DisplayName,
            VanityUrl = x.Entry.VanityUrl,
            AvatarUrl = x.Entry.AvatarUrl,
            BestStreak = x.Streak,
            TotalMatches = x.Entry.TotalMatches
        }).ToList();

        return new LeaderboardResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            CalculatedAt = _lastCalculated
        };
    }

    private static int CalculateBestStreak(IEnumerable<MatchResult> results)
    {
        var best = 0;
        var current = 0;
        foreach (var result in results)
        {
            if (result == MatchResult.Win)
            {
                current++;
                if (current > best) best = current;
            }
            else
            {
                current = 0;
            }
        }
        return best;
    }
}

public class LeaderboardEntry
{
    public int UserId { get; set; }
    public required string SteamId { get; set; }
    public required string DisplayName { get; set; }
    public string? VanityUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public int BestOverall { get; set; }
    public int BestKiller { get; set; }
    public int BestSurvivor { get; set; }
    public Dictionary<string, int> PerKillerBest { get; set; } = [];
    public Dictionary<string, PeriodStreak> PeriodStreaks { get; set; } = [];
    public int TotalMatches { get; set; }
}

public class PeriodStreak
{
    public int Overall { get; set; }
    public int Killer { get; set; }
    public int Survivor { get; set; }
    public Dictionary<string, int> PerKiller { get; set; } = [];
}

public class LeaderboardItem
{
    public int Rank { get; set; }
    public required string SteamId { get; set; }
    public required string DisplayName { get; set; }
    public string? VanityUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public int BestStreak { get; set; }
    public int TotalMatches { get; set; }
}

public class LeaderboardResult
{
    public List<LeaderboardItem> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public DateTimeOffset CalculatedAt { get; set; }
}
