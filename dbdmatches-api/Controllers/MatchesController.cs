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
    public async Task<IActionResult> GetHistory([FromQuery] string role = "all", [FromQuery] string? killer = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;
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
            var survivorMatches = await db.MatchSurvivors
                .Where(m => m.UserId == user.Id)
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
}
