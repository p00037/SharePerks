using Shared.Entities;

namespace User.Data.Repositories;

public interface IRewardItemRepository
{
    Task<IReadOnlyList<RewardItem>> ListActiveAsync(CancellationToken cancellationToken = default);
}
