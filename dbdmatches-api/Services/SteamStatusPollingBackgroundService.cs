using DbdMatches.Api.Data;
using DbdMatches.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DbdMatches.Api.Services;

public class SteamStatusPollingBackgroundService(
    IServiceScopeFactory scopeFactory,
    SteamStatusCacheService statusCache,
    ILogger<SteamStatusPollingBackgroundService> logger) : BackgroundService
{

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
                    .Select(u => new { u.Id, u.SteamId })
                    .ToListAsync(stoppingToken);

                var steamIdsToPoll = new List<string>();

                var pollInGame = now - _lastInGamePoll >= SteamStatusCacheService.InGamePollingInterval;
                var pollOnline = now - _lastOnlinePoll >= SteamStatusCacheService.OnlinePollingInterval;
                var pollOffline = now - _lastOfflinePoll >= SteamStatusCacheService.OfflinePollingInterval;

                foreach (var user in allUsers)
                {
                    var entry = statusCache.Get(user.SteamId);

                    var shouldPoll = entry.Status switch
                    {
                        SteamStatus.InGame => pollInGame,
                        SteamStatus.Online => pollOnline,
                        _ => pollOffline
                    };

                    if (shouldPoll)
                        steamIdsToPoll.Add(user.SteamId);
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

                // TODO: In production, only run DetectMatch for InGame users
                // if (pollInGame)
                // {
                //     var matchDetection = scope.ServiceProvider.GetRequiredService<MatchDetectionService>();
                //     var inGameUsers = allUsers
                //         .Where(u => statusCache.Get(u.SteamId).Status == SteamStatus.InGame)
                //         .ToList();
                //
                //     foreach (var user in inGameUsers)
                //     {
                //         try
                //         {
                //             await matchDetection.DetectMatch(db, user.Id, user.SteamId);
                //         }
                //         catch (Exception ex)
                //         {
                //             logger.LogError(ex, "Error detecting match for user {UserId}", user.Id);
                //         }
                //     }
                // }

                {
                    var matchDetection = scope.ServiceProvider.GetRequiredService<MatchDetectionService>();
                    foreach (var user in allUsers)
                    {
                        try
                        {
                            await matchDetection.DetectMatch(db, user.Id, user.SteamId);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error detecting match for user {UserId}", user.Id);
                        }
                    }
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
