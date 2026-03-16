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
    int BloodpointsEarned,
    KillerMatchDetails? Killer,
    SurvivorMatchDetails? Survivor
);

public record KillerMatchDetails(
    string KillerName,
    int Sacrifices,
    int Kills,
    int PowerStat1,
    string PowerStat1Label,
    int PowerStat2,
    string? PowerStat2Label,
    int PowerStat3,
    string? PowerStat3Label
);

public record SurvivorMatchDetails(
    bool Escaped,
    bool HatchEscape,
    double GeneratorsCompleted
);
