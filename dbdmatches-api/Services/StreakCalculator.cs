using DbdMatches.Api.Models;

namespace DbdMatches.Api.Services;

public static class StreakCalculator
{
    public record StreakResult(int Current, int Best);

    public record StreaksByRole(
        StreakResult Overall,
        StreakResult Killer,
        StreakResult Survivor,
        List<KillerStreakResult> Killers);

    public record KillerStreakResult(string Killer, int Current, int Best);

    public static StreaksByRole Calculate(
        List<MatchKiller> killerMatches,
        List<MatchSurvivor> survivorMatches)
    {
        var allMatches = killerMatches
            .Select(m => new { m.PlayedAt, m.Result, Role = "killer", Killer = m.Killer })
            .Concat(survivorMatches.Select(m => new { m.PlayedAt, m.Result, Role = "survivor", Killer = (string?)null }))
            .OrderBy(m => m.PlayedAt)
            .ToList();

        var overallStreak = CalculateStreak(allMatches.Select(m => m.Result));
        var killerStreak = CalculateStreak(allMatches.Where(m => m.Role == "killer").Select(m => m.Result));
        var survivorStreak = CalculateStreak(allMatches.Where(m => m.Role == "survivor").Select(m => m.Result));

        var killerNames = allMatches.Where(m => m.Killer != null).Select(m => m.Killer!).Distinct().ToList();
        var perKillerStreaks = killerNames
            .Select(name => new KillerStreakResult(
                name,
                CalculateStreak(allMatches.Where(m => m.Killer == name).Select(m => m.Result)).Current,
                CalculateStreak(allMatches.Where(m => m.Killer == name).Select(m => m.Result)).Best))
            .ToList();

        return new StreaksByRole(overallStreak, killerStreak, survivorStreak, perKillerStreaks);
    }

    private static StreakResult CalculateStreak(IEnumerable<MatchResult> results)
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
        return new StreakResult(current, best);
    }
}
