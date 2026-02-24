using Shared.Entities;

namespace Shareholder.Data.Repositories;

public interface IRewardOrderRepository
{
    Task<bool> ExistsByShareholderIdAsync(int shareholderId, CancellationToken cancellationToken = default);
    void Add(RewardOrder rewardOrder);
}
