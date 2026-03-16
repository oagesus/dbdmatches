namespace DbdMatches.Api.Services;

public class LeaderboardBackgroundService(
    LeaderboardService leaderboardService,
    ILogger<LeaderboardBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        await CalculateLeaderboard();

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var nextHour = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, TimeSpan.Zero)
                .AddHours(1);
            var delay = nextHour - now - TimeSpan.FromSeconds(30);

            logger.LogInformation("Next leaderboard calculation at {NextHour} (in {Minutes} minutes)",
                nextHour, delay.TotalMinutes);

            await Task.Delay(delay, stoppingToken);
            await CalculateLeaderboard();
        }
    }

    private async Task CalculateLeaderboard()
    {
        try
        {
            await leaderboardService.CalculateAsync();
            logger.LogInformation("Leaderboard calculation completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Leaderboard calculation failed");
        }
    }
}
