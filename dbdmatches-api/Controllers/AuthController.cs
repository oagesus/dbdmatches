using DbdMatches.Api.Data;
using DbdMatches.Api.DTOs;
using DbdMatches.Api.Models;
using DbdMatches.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbdMatches.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AppDbContext db,
    JwtService jwtService,
    SteamAuthService steamAuthService,
    SteamStatusCacheService statusCache,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("steam/login")]
    public IActionResult SteamLogin()
    {
        var frontendUrl = configuration["Frontend:Url"]
            ?? throw new InvalidOperationException("Frontend URL not configured");
        var apiUrl = configuration["Api:Url"]
            ?? throw new InvalidOperationException("API URL not configured");

        var returnUrl = $"{apiUrl}/api/auth/steam/callback";
        var loginUrl = steamAuthService.GetLoginUrl(returnUrl);
        return Redirect(loginUrl);
    }

    [HttpGet("steam/callback")]
    public async Task<IActionResult> SteamCallback()
    {
        var frontendUrl = configuration["Frontend:Url"]
            ?? throw new InvalidOperationException("Frontend URL not configured");

        var parameters = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());

        var steamId = await steamAuthService.ValidateOpenIdResponse(parameters);
        if (steamId == null)
            return Redirect($"{frontendUrl}/auth/login?error=steam_auth_failed");

        var playerSummary = await steamAuthService.GetPlayerSummary(steamId);
        if (playerSummary == null)
            return Redirect($"{frontendUrl}/auth/login?error=steam_profile_failed");

        var user = await db.Users.FirstOrDefaultAsync(u => u.SteamId == steamId);

        if (user == null)
        {
            user = new User
            {
                SteamId = steamId,
                DisplayName = playerSummary.PersonaName,
                AvatarUrl = playerSummary.AvatarFull
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }
        else
        {
            user.DisplayName = playerSummary.PersonaName;
            user.AvatarUrl = playerSummary.AvatarFull;
            await db.SaveChangesAsync();
        }

        var playerInfos = await steamAuthService.GetBulkPlayerStatuses([steamId]);
        if (playerInfos.TryGetValue(steamId, out var playerInfo))
            statusCache.Set(steamId, playerInfo.Status);

        if (user.IsBlocked)
            return Redirect($"{frontendUrl}/auth/login?error=account_blocked");

        var session = new UserSession
        {
            UserId = user.Id,
            DeviceInfo = Request.Headers.UserAgent.ToString()
        };
        db.UserSessions.Add(session);
        await db.SaveChangesAsync();

        var accessToken = jwtService.GenerateAccessToken(user);
        var (refreshToken, refreshTokenHash) = jwtService.GenerateRefreshToken();

        var refreshTokenExpirationDays = int.Parse(configuration["Jwt:RefreshTokenExpirationDays"] ?? "30");

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            SessionId = session.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshTokenExpirationDays)
        });
        await db.SaveChangesAsync();

        SetAuthCookies(accessToken, refreshToken);

        return Redirect($"{frontendUrl}/dashboard");
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshTokenValue = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshTokenValue))
            return Unauthorized(new { error = "No refresh token" });

        var tokenHash = jwtService.HashToken(refreshTokenValue);

        var refreshToken = await db.RefreshTokens
            .Include(t => t.User)
            .Include(t => t.Session)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (refreshToken == null || refreshToken.ExpiresAt < DateTimeOffset.UtcNow)
            return Unauthorized(new { error = "Invalid or expired refresh token" });

        if (refreshToken.User.IsBlocked)
            return StatusCode(403, new { error = "Account is blocked" });

        db.RefreshTokens.Remove(refreshToken);

        var newAccessToken = jwtService.GenerateAccessToken(refreshToken.User);
        var (newRefreshToken, newRefreshTokenHash) = jwtService.GenerateRefreshToken();

        var refreshTokenExpirationDays = int.Parse(configuration["Jwt:RefreshTokenExpirationDays"] ?? "30");

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = refreshToken.UserId,
            SessionId = refreshToken.SessionId,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshTokenExpirationDays)
        });

        refreshToken.Session.LastActivityAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        SetAuthCookies(newAccessToken, newRefreshToken);

        return Ok(new { message = "Token refreshed" });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshTokenValue = Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            var tokenHash = jwtService.HashToken(refreshTokenValue);
            var refreshToken = await db.RefreshTokens
                .Include(t => t.Session)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (refreshToken != null)
            {
                refreshToken.Session.LoggedOutAt = DateTimeOffset.UtcNow;
                db.RefreshTokens.Remove(refreshToken);
                await db.SaveChangesAsync();
            }
        }

        ClearAuthCookies();
        return Ok(new { message = "Logged out" });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var publicIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (publicIdClaim == null || !Guid.TryParse(publicIdClaim, out var publicId))
            return Unauthorized();

        var user = await db.Users.FirstOrDefaultAsync(u => u.PublicId == publicId);
        if (user == null)
            return NotFound();

        var entry = statusCache.Get(user.SteamId);
        var interval = statusCache.GetInterval(entry.Status);
        var intervalMinutes = (int)interval.TotalMinutes;
        var now = DateTimeOffset.UtcNow;
        var currentMinute = now.Minute;
        var currentSecond = now.Second;
        var minutesUntilNext = intervalMinutes - (currentMinute % intervalMinutes);
        if (minutesUntilNext == intervalMinutes) minutesUntilNext = 0;
        var nextUpdateSeconds = (minutesUntilNext * 60) - currentSecond;
        if (nextUpdateSeconds <= 0) nextUpdateSeconds = intervalMinutes * 60;

        return Ok(new UserResponse(
            user.PublicId,
            user.SteamId,
            user.DisplayName,
            user.AvatarUrl,
            entry.Status.ToString(),
            nextUpdateSeconds,
            user.CreatedAt
        ));
    }

    private void SetAuthCookies(string accessToken, string refreshToken)
    {
        var isProduction = !string.Equals(
            configuration["ASPNETCORE_ENVIRONMENT"], "Development",
            StringComparison.OrdinalIgnoreCase);

        var accessTokenExpirationMinutes = int.Parse(configuration["Jwt:AccessTokenExpirationMinutes"] ?? "5");
        var refreshTokenExpirationDays = int.Parse(configuration["Jwt:RefreshTokenExpirationDays"] ?? "30");

        Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromMinutes(accessTokenExpirationMinutes)
        });

        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromDays(refreshTokenExpirationDays)
        });

        var tokenExp = DateTimeOffset.UtcNow.AddMinutes(accessTokenExpirationMinutes).ToUnixTimeSeconds();
        Response.Cookies.Append("token_exp", tokenExp.ToString(), new CookieOptions
        {
            HttpOnly = false,
            Secure = isProduction,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromDays(refreshTokenExpirationDays)
        });
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        Response.Cookies.Delete("token_exp");
    }
}
