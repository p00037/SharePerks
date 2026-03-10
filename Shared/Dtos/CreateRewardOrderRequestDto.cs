namespace Shared.Dtos;

public sealed record CreateRewardOrderRequestDto(
    string PostalCode,
    string Address1,
    string Address2,
    string? Address3,
    string PhoneNumber,
    IReadOnlyList<CreateRewardOrderItemRequestDto> Items);

public sealed record CreateRewardOrderItemRequestDto(int ItemId, int Quantity);
