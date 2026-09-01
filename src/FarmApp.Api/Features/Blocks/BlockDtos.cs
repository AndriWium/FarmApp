namespace FarmApp.Api.Features.Blocks;

public record BlockDto(
    int BlockId,
    string Name,
    decimal AreaHectare,
    string Note,
    bool IsActive
    );

public record CreateBlockRequest(
    string Name,
    decimal AreaHectare,
    string Note
    );

public record UpdateBlockRequest(
    string Name,
    decimal AreaHectare,
    string Note,
    bool IsActive
    );
