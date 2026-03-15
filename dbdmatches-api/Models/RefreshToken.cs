namespace DbdMatches.Api.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SessionId { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public UserSession Session { get; set; } = null!;
}
