using DbdMatches.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DbdMatches.Api.Filters;

public class BlockedUserFilter(AppDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdClaim = context.HttpContext.User.FindFirst("sub")?.Value
            ?? context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var publicId))
        {
            var isBlocked = await db.Users
                .AnyAsync(u => u.PublicId == publicId && u.IsBlocked);

            if (isBlocked)
            {
                context.Result = new ObjectResult(new { error = "Account is blocked" })
                {
                    StatusCode = 403
                };
                return;
            }
        }

        await next();
    }
}
