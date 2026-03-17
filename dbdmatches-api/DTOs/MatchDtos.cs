namespace DbdMatches.Api.DTOs;

public record MatchHistoryResponse(
    List<MatchHistoryItem> Matches,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record MatchHistoryItem(
    Guid PublicId,
    string Role,
    string Result,
    DateTimeOffset PlayedAt,
    bool IsContaminated,
    KillerMatchDetails? Killer,
    SurvivorMatchDetails? Survivor
);

public record KillerMatchDetails(
    string KillerName,
    int Sacrifices,
    int Kills
);

public record SurvivorMatchDetails(
    bool Escaped,
    bool HatchEscape
);
