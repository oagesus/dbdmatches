using DbdMatches.Api.Data;
using DbdMatches.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DbdMatches.Api.Services;

public class SteamStatusPollingBackgroundService(
    IServiceScopeFactory scopeFactory,
    SteamStatusCacheService statusCache,
    ILogger<SteamStatusPollingBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan InGamePollingInterval = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan OnlinePollingInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan OfflinePollingInterval = TimeSpan.FromMinutes(10);

    private DateTimeOffset _lastInGamePoll = DateTimeOffset.MinValue;
    private DateTimeOffset _lastOnlinePoll = DateTimeOffset.MinValue;
    private DateTimeOffset _lastOfflinePoll = DateTimeOffset.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var steamAuthService = scope.ServiceProvider.GetRequiredService<SteamAuthService>();

                var allUsers = await db.Users
                    .Where(u => !u.IsBlocked)
                    .Select(u => u.SteamId)
                    .ToListAsync(stoppingToken);

                var steamIdsToPoll = new List<string>();

                var pollInGame = now - _lastInGamePoll >= InGamePollingInterval;
                var pollOnline = now - _lastOnlinePoll >= OnlinePollingInterval;
                var pollOffline = now - _lastOfflinePoll >= OfflinePollingInterval;

                foreach (var steamId in allUsers)
                {
                    var entry = statusCache.Get(steamId);

                    var shouldPoll = entry.Status switch
                    {
                        SteamStatus.InGame => pollInGame,
                        SteamStatus.Online => pollOnline,
                        _ => pollOffline
                    };

                    if (shouldPoll)
                        steamIdsToPoll.Add(steamId);
                }

                if (steamIdsToPoll.Count > 0)
                {
                    var statuses = await steamAuthService.GetBulkPlayerStatuses(steamIdsToPoll);
                    statusCache.SetBulk(statuses);

                    if (pollInGame) _lastInGamePoll = now;
                    if (pollOnline) _lastOnlinePoll = now;
                    if (pollOffline) _lastOfflinePoll = now;

                    logger.LogInformation("Polled status for {Count} users", steamIdsToPoll.Count);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error polling Steam statuses");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
