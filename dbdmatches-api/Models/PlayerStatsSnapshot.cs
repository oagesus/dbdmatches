namespace DbdMatches.Api.Models;

public class PlayerStatsSnapshot
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string StatName { get; set; }
    public double StatValue { get; set; }
    public DateTimeOffset FetchedAt { get; set; }

    public User User { get; set; } = null!;
}
