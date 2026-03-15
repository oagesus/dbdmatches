namespace DbdMatches.Api.DTOs;

public record UserResponse(
    Guid PublicId,
    string SteamId,
    string DisplayName,
    string? AvatarUrl,
    string Status,
    int NextUpdateSeconds,
    DateTimeOffset CreatedAt
);

public record SteamPlayerSummary(
    string SteamId,
    string PersonaName,
    string? AvatarFull
);
