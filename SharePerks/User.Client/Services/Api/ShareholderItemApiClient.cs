using Shared.Dtos;
using User.Client.Services.Api.Interface;

namespace User.Client.Services.Api;

public sealed class ShareholderItemApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IShareholderItemApiClient
{
    public Task<IReadOnlyList<RewardItemSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyList<RewardItemSummaryDto>>(
            "api/shareholder/items",
            failedMessage: "優待商品の取得に失敗しました。",
            cancellationToken: cancellationToken);
    }
}
