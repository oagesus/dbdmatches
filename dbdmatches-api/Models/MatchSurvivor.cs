namespace DbdMatches.Api.Models;

public class MatchSurvivor
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int UserId { get; set; }
    public bool Escaped { get; set; }
    public bool HatchEscape { get; set; }
    public double GeneratorsCompleted { get; set; }
    public int BloodpointsEarned { get; set; }
    public MatchResult Result { get; set; }
    public DateTimeOffset PlayedAt { get; set; }

    public User User { get; set; } = null!;
}
