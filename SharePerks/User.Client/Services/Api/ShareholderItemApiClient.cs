using Shared.Dtos;
using User.Client.Services.Api.Interface;

namespace User.Client.Services.Api;

public sealed class ShareholderItemApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IShareholderItemApiClient
{
    public Task<IReadOnlyList<RewardItemSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyList<RewardItemSummaryDto>>(
            "api/shareholder/items",
            failedMessage: "蜆ｪ蠕・膚蜩√・蜿門ｾ励↓螟ｱ謨励＠縺ｾ縺励◆縲・",
            cancellationToken: cancellationToken);
    }
}
