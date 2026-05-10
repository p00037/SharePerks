namespace Shared.Dtos;

public sealed record RewardItemBulkUpdateRequestDto(
    IReadOnlyList<RewardItemBulkUpdateRowDto> Items);

public sealed record RewardItemBulkUpdateRowDto(
    int ItemId,
    int RequiredPoints,
    int DisplayOrder);
