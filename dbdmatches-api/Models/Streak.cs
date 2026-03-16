namespace DbdMatches.Api.Models;

public class Streak
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CurrentOverall { get; set; }
    public int BestOverall { get; set; }
    public int CurrentKiller { get; set; }
    public int BestKiller { get; set; }
    public int CurrentSurvivor { get; set; }
    public int BestSurvivor { get; set; }

    public User User { get; set; } = null!;
}
