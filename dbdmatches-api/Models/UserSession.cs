namespace DbdMatches.Api.Models;

public class UserSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? DeviceInfo { get; set; }
    public DateTimeOffset LoggedInAt { get; set; }
    public DateTimeOffset LastActivityAt { get; set; }
    public DateTimeOffset? LoggedOutAt { get; set; }

    public User User { get; set; } = null!;
}
