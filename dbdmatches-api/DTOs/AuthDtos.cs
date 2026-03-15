namespace DbdMatches.Api.DTOs;

public record UserResponse(
    Guid PublicId,
    string SteamId,
    string DisplayName,
    string? AvatarUrl,
    DateTimeOffset CreatedAt
);

public record SteamPlayerSummary(
    string SteamId,
    string PersonaName,
    string? AvatarFull
);
