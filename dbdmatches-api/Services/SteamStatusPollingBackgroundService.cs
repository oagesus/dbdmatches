using DbdMatches.Api.Data;
using DbdMatches.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DbdMatches.Api.Services;

public class SteamStatusPollingBackgroundService(
    IServiceScopeFactory scopeFactory,
    SteamStatusCacheService statusCache,
    ILogger<SteamStatusPollingBackgroundService> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var secondsUntilNext = 60 - DateTimeOffset.UtcNow.Second - 5;
        if (secondsUntilNext <= 0) secondsUntilNext += 60;
        await Task.Delay(TimeSpan.FromSeconds(secondsUntilNext), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var currentMinute = (now.Second >= 55) ? (now.Minute + 1) % 60 : now.Minute;

                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var steamAuthService = scope.ServiceProvider.GetRequiredService<SteamAuthService>();

                var allUsers = await db.Users
                    .Where(u => !u.IsBlocked)
                    .ToListAsync(stoppingToken);

                var steamIdsToPoll = new List<string>();

                var inGameMinutes = (int)SteamStatusCacheService.InGamePollingInterval.TotalMinutes;
                var onlineMinutes = (int)SteamStatusCacheService.OnlinePollingInterval.TotalMinutes;
                var offlineMinutes = (int)SteamStatusCacheService.OfflinePollingInterval.TotalMinutes;

                var pollInGame = currentMinute % inGameMinutes == 0;
                var pollOnline = currentMinute % onlineMinutes == 0;
                var pollOffline = currentMinute % offlineMinutes == 0;

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
                    var playerInfos = await steamAuthService.GetBulkPlayerStatuses(steamIdsToPoll);
                    statusCache.SetBulk(playerInfos);

                    foreach (var user in allUsers)
                    {
                        if (playerInfos.TryGetValue(user.SteamId, out var info))
                        {
                            if (user.DisplayName != info.DisplayName || user.AvatarUrl != info.AvatarUrl || user.VanityUrl != info.VanityUrl)
                            {
                                user.DisplayName = info.DisplayName;
                                user.AvatarUrl = info.AvatarUrl;
                                user.VanityUrl = info.VanityUrl;
                            }
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);

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
                    var usersToDetect = allUsers.ToList();
                    var availableMs = 110_000;
                    var delayMs = usersToDetect.Count > 1 ? availableMs / usersToDetect.Count : 0;
                    delayMs = Math.Clamp(delayMs, 0, 1000);

                    for (var i = 0; i < usersToDetect.Count; i++)
                    {
                        var user = usersToDetect[i];
                        try
                        {
                            await matchDetection.DetectMatch(db, user.Id, user.SteamId);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error detecting match for user {UserId}", user.Id);
                        }

                        if (delayMs > 0 && i < usersToDetect.Count - 1)
                            await Task.Delay(delayMs, stoppingToken);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error polling Steam statuses");
            }

            var secsUntilNext = 60 - DateTimeOffset.UtcNow.Second - 5;
            if (secsUntilNext <= 0) secsUntilNext += 60;
            await Task.Delay(TimeSpan.FromSeconds(secsUntilNext), stoppingToken);
        }
    }
}
