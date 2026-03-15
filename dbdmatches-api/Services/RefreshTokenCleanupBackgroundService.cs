using DbdMatches.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DbdMatches.Api.Services;

public class RefreshTokenCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<RefreshTokenCleanupBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(3); // 3:00 AM UTC
            if (nextRun <= now)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var deleted = await db.RefreshTokens
                    .Where(t => t.ExpiresAt < DateTimeOffset.UtcNow)
                    .ExecuteDeleteAsync(stoppingToken);

                logger.LogInformation("Cleaned up {Count} expired refresh tokens", deleted);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error cleaning up expired refresh tokens");
            }
        }
    }
}
