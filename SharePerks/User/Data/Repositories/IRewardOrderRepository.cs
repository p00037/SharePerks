using Shared.Entities;

namespace User.Data.Repositories;

public interface IRewardOrderRepository
{
    Task<bool> ExistsByShareholderIdAsync(int shareholderId, CancellationToken cancellationToken = default);
    Task<RewardOrder?> GetByShareholderIdAsync(int shareholderId, CancellationToken cancellationToken = default);
    Task<RewardOrder?> GetByShareholderIdForUpdateAsync(int shareholderId, CancellationToken cancellationToken = default);
    void Add(RewardOrder rewardOrder);
    void ReplaceItems(RewardOrder rewardOrder, IReadOnlyCollection<RewardOrderItem> orderItems);
}
