using Shared.Dtos;

namespace User.Client.Services.Api.Interface;

public interface IShareholderItemApiClient
{
    Task<IReadOnlyList<RewardItemSummaryDto>> ListAsync(CancellationToken cancellationToken = default);
}
