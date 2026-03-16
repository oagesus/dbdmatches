using DbdMatches.Api.Data;
using DbdMatches.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbdMatches.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController(LeaderboardService leaderboardService, AppDbContext db) : ControllerBase
{
    [HttpGet]
    public IActionResult GetLeaderboard(
        [FromQuery] string role = "all",
        [FromQuery] string? killer = null,
        [FromQuery] string? period = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var result = leaderboardService.GetLeaderboard(role, killer, period, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{steamId}/matches")]
    public async Task<IActionResult> GetPlayerMatches(
        string steamId,
        [FromQuery] string role = "all",
        [FromQuery] string? killer = null,
        [FromQuery] string? period = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var user = await db.Users.FirstOrDefaultAsync(u => u.SteamId == steamId && !u.IsBlocked);
        if (user == null)
            return NotFound();

        DateTimeOffset? since = period switch
        {
            "30d" => DateTimeOffset.UtcNow.AddDays(-30),
            "90d" => DateTimeOffset.UtcNow.AddDays(-90),
            "1y" => DateTimeOffset.UtcNow.AddYears(-1),
            _ => null
        };

        var items = new List<object>();

        if (role is "all" or "killer")
        {
            var killerQuery = db.MatchKillers.Where(m => m.UserId == user.Id);
            if (!string.IsNullOrEmpty(killer))
                killerQuery = killerQuery.Where(m => m.Killer == killer);
            if (since.HasValue)
                killerQuery = killerQuery.Where(m => m.PlayedAt >= since.Value);

            var killerMatches = await killerQuery.OrderByDescending(m => m.PlayedAt).ToListAsync();
            foreach (var m in killerMatches)
            {
                var killerInfo = KillerMappingService.GetByName(m.Killer);
                items.Add(new
                {
                    publicId = m.PublicId,
                    role = "killer",
                    result = m.Result.ToString(),
                    playedAt = m.PlayedAt,
                    bloodpointsEarned = m.BloodpointsEarned,
                    killer = new
                    {
                        killerName = m.Killer,
                        sacrifices = m.Sacrifices,
                        kills = m.Kills,
                        powerStat1 = m.PowerStat1,
                        powerStat1Label = killerInfo?.Stat1Label ?? "Power stat 1",
                        powerStat2 = m.PowerStat2,
                        powerStat2Label = killerInfo?.Stat2Label,
                        powerStat3 = m.PowerStat3,
                        powerStat3Label = killerInfo?.Stat3Label
                    },
                    survivor = (object?)null
                });
            }
        }

        if (role is "all" or "survivor")
        {
            var survivorQuery = db.MatchSurvivors.Where(m => m.UserId == user.Id);
            if (since.HasValue)
                survivorQuery = survivorQuery.Where(m => m.PlayedAt >= since.Value);

            var survivorMatches = await survivorQuery.OrderByDescending(m => m.PlayedAt).ToListAsync();
            foreach (var m in survivorMatches)
            {
                items.Add(new
                {
                    publicId = m.PublicId,
                    role = "survivor",
                    result = m.Result.ToString(),
                    playedAt = m.PlayedAt,
                    bloodpointsEarned = m.BloodpointsEarned,
                    killer = (object?)null,
                    survivor = new
                    {
                        escaped = m.Escaped,
                        hatchEscape = m.HatchEscape,
                        generatorsCompleted = m.GeneratorsCompleted
                    }
                });
            }
        }

        var sorted = items.Cast<dynamic>().OrderByDescending(i => (DateTimeOffset)i.playedAt).ToList();
        var totalCount = sorted.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var paged = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        object streaksData;
        if (since.HasValue)
        {
            var allKillerMatches = await db.MatchKillers
                .Where(m => m.UserId == user.Id && m.PlayedAt >= since.Value)
                .OrderBy(m => m.PlayedAt)
                .ToListAsync();
            var allSurvivorMatches = await db.MatchSurvivors
                .Where(m => m.UserId == user.Id && m.PlayedAt >= since.Value)
                .OrderBy(m => m.PlayedAt)
                .ToListAsync();

            var calc = StreakCalculator.Calculate(allKillerMatches, allSurvivorMatches);
            streaksData = new
            {
                overall = new { current = calc.Overall.Current, best = calc.Overall.Best },
                killer = new { current = calc.Killer.Current, best = calc.Killer.Best },
                survivor = new { current = calc.Survivor.Current, best = calc.Survivor.Best },
                killers = calc.Killers.Select(k => new { killer = k.Killer, current = k.Current, best = k.Best })
            };
        }
        else
        {
            var streak = await db.Streaks.FirstOrDefaultAsync(s => s.UserId == user.Id);
            var killerStreaks = await db.StreakKillers
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.BestStreak)
                .ToListAsync();

            streaksData = new
            {
                overall = new { current = streak?.CurrentOverall ?? 0, best = streak?.BestOverall ?? 0 },
                killer = new { current = streak?.CurrentKiller ?? 0, best = streak?.BestKiller ?? 0 },
                survivor = new { current = streak?.CurrentSurvivor ?? 0, best = streak?.BestSurvivor ?? 0 },
                killers = killerStreaks.Select(k => new { killer = k.Killer, current = k.CurrentStreak, best = k.BestStreak })
            };
        }

        return Ok(new
        {
            matches = paged,
            totalCount,
            page,
            pageSize,
            totalPages,
            player = new
            {
                steamId = user.SteamId,
                displayName = user.DisplayName,
                avatarUrl = user.AvatarUrl
            },
            streaks = streaksData
        });
    }
}
