namespace DbdMatches.Api.Models;

public class MatchKiller
{
    public int Id { get; set; }
    public Guid PublicId { get; set; }
    public int UserId { get; set; }
    public required string Killer { get; set; }
    public int Sacrifices { get; set; }
    public int Kills { get; set; }
    public int PowerStat1 { get; set; }
    public int PowerStat2 { get; set; }
    public int PowerStat3 { get; set; }
    public int BloodpointsEarned { get; set; }
    public MatchResult Result { get; set; }
    public bool IsContaminated { get; set; }
    public DateTimeOffset PlayedAt { get; set; }

    public User User { get; set; } = null!;
}
