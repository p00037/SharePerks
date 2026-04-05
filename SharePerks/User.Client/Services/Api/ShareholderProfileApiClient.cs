using Shared.Dtos;
using User.Client.Services.Api.Interface;

namespace User.Client.Services.Api;

public sealed class ShareholderProfileApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IShareholderProfileApiClient
{
    public Task<ShareholderAddressDto> GetAddressAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<ShareholderAddressDto>(
            "api/shareholder/profile/address",
            failedMessage: "株主住所の取得に失敗しました。",
            cancellationToken: cancellationToken);
    }
}
