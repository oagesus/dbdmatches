namespace DbdMatches.Api.Models;

public class User
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public required string SteamId { get; set; }
    public required string DisplayName { get; set; }
    public string? VanityUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsBlocked { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = [];
    public List<UserSession> Sessions { get; set; } = [];
    public List<MatchSurvivor> SurvivorMatches { get; set; } = [];
    public List<MatchKiller> KillerMatches { get; set; } = [];
    public List<PlayerStatsSnapshot> StatsSnapshots { get; set; } = [];
    public Streak? Streak { get; set; }
    public List<StreakKiller> StreakKillers { get; set; } = [];
}
