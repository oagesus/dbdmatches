namespace DbdMatches.Api.Models;

public class StreakKiller
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Killer { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }

    public User User { get; set; } = null!;
}
