using Shared.Entities;

namespace Shareholder.Data.Repositories;

public interface IRewardItemRepository
{
    Task<IReadOnlyList<RewardItem>> ListActiveAsync(CancellationToken cancellationToken = default);
}
