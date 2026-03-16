using DbdMatches.Api.Data;
using DbdMatches.Api.DTOs;
using DbdMatches.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbdMatches.Api.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController(AppDbContext db) : ControllerBase
{
    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] string role = "all", [FromQuery] string? killer = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? period = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        DateTimeOffset? since = period switch
        {
            "30d" => DateTimeOffset.UtcNow.AddDays(-30),
            "90d" => DateTimeOffset.UtcNow.AddDays(-90),
            "1y" => DateTimeOffset.UtcNow.AddYears(-1),
            _ => null
        };
        var publicIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var publicId))
            return Unauthorized();

        var user = await db.Users.FirstOrDefaultAsync(u => u.PublicId == publicId);
        if (user == null)
            return NotFound();

        var items = new List<MatchHistoryItem>();

        if (role is "all" or "killer")
        {
            var killerQuery = db.MatchKillers
                .Where(m => m.UserId == user.Id);

            if (!string.IsNullOrEmpty(killer))
                killerQuery = killerQuery.Where(m => m.Killer == killer);

            if (since.HasValue)
                killerQuery = killerQuery.Where(m => m.PlayedAt >= since.Value);

            var killerMatches = await killerQuery
                .OrderByDescending(m => m.PlayedAt)
                .ToListAsync();

            foreach (var m in killerMatches)
            {
                var killerInfo = KillerMappingService.GetByName(m.Killer);

                items.Add(new MatchHistoryItem(
                    m.PublicId,
                    "killer",
                    m.Result.ToString(),
                    m.PlayedAt,
                    m.BloodpointsEarned,
                    new KillerMatchDetails(
                        m.Killer,
                        m.Sacrifices,
                        m.Kills,
                        m.PowerStat1,
                        killerInfo?.Stat1Label ?? "Power stat 1",
                        m.PowerStat2,
                        killerInfo?.Stat2Label,
                        m.PowerStat3,
                        killerInfo?.Stat3Label
                    ),
                    null
                ));
            }
        }

        if (role is "all" or "survivor")
        {
            var survivorQuery = db.MatchSurvivors
                .Where(m => m.UserId == user.Id);

            if (since.HasValue)
                survivorQuery = survivorQuery.Where(m => m.PlayedAt >= since.Value);

            var survivorMatches = await survivorQuery
                .OrderByDescending(m => m.PlayedAt)
                .ToListAsync();

            foreach (var m in survivorMatches)
            {
                items.Add(new MatchHistoryItem(
                    m.PublicId,
                    "survivor",
                    m.Result.ToString(),
                    m.PlayedAt,
                    m.BloodpointsEarned,
                    null,
                    new SurvivorMatchDetails(
                        m.Escaped,
                        m.HatchEscape,
                        m.GeneratorsCompleted
                    )
                ));
            }
        }

        var sorted = items.OrderByDescending(i => i.PlayedAt).ToList();
        var totalCount = sorted.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var paged = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new MatchHistoryResponse(paged, totalCount, page, pageSize, totalPages));
    }

    [HttpGet("killers")]
    public IActionResult GetAllKillers()
    {
        var killers = KillerMappingService.GetAllKillerNames();
        return Ok(killers);
    }

    [Authorize]
    [HttpGet("streaks")]
    public async Task<IActionResult> GetStreaks()
    {
        var publicIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var publicId))
            return Unauthorized();

        var user = await db.Users.FirstOrDefaultAsync(u => u.PublicId == publicId);
        if (user == null)
            return NotFound();

        var streak = await db.Streaks.FirstOrDefaultAsync(s => s.UserId == user.Id);
        var killerStreaks = await db.StreakKillers
            .Where(s => s.UserId == user.Id)
            .OrderByDescending(s => s.BestStreak)
            .ToListAsync();

        return Ok(new
        {
            overall = new { current = streak?.CurrentOverall ?? 0, best = streak?.BestOverall ?? 0 },
            killer = new { current = streak?.CurrentKiller ?? 0, best = streak?.BestKiller ?? 0 },
            survivor = new { current = streak?.CurrentSurvivor ?? 0, best = streak?.BestSurvivor ?? 0 },
            killers = killerStreaks.Select(k => new
            {
                killer = k.Killer,
                current = k.CurrentStreak,
                best = k.BestStreak
            })
        });
    }
}
