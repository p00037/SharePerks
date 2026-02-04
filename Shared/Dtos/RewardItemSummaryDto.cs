namespace Shared.Dtos;

public sealed record RewardItemSummaryDto(
    int ItemId,
    string ItemCode,
    string ItemName,
    string? ItemDescription,
    int RequiredPoints,
    string? ImagePath
);
