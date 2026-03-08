using Shared.Entities;

namespace User.Data.Repositories;

public interface IRewardOrderRepository
{
    Task<bool> ExistsByShareholderIdAsync(int shareholderId, CancellationToken cancellationToken = default);
    void Add(RewardOrder rewardOrder);
}
