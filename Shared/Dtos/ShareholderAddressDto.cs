namespace Shared.Dtos;

public sealed record ShareholderAddressDto(
    string PostalCode,
    string Address1,
    string Address2,
    string? Address3,
    string? PhoneNumber);
