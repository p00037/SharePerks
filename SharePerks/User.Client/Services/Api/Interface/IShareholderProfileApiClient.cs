using Shared.Dtos;

namespace User.Client.Services.Api.Interface;

public interface IShareholderProfileApiClient
{
    Task<ShareholderAddressDto> GetAddressAsync(CancellationToken cancellationToken = default);
}
