namespace Shared.Dtos;

public sealed record ShareholderOrderDto(
    int OrderId,
    string PostalCode,
    string Address1,
    string Address2,
    string? Address3,
    string? PhoneNumber,
    int TotalPoints,
    bool IsExported,
    IReadOnlyList<ShareholderOrderItemDto> Items);

public sealed record ShareholderOrderItemDto(
    int ItemId,
    string ItemCode,
    string ItemName,
    string? ItemDescription,
    string? ImagePath,
    int RequiredPoints,
    int Quantity);
