using Shared.Entities;

namespace Admin.Data.Repositories;

public interface IRewardItemRepository
{
    Task<RewardItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByItemCodeAsync(string itemCode, CancellationToken cancellationToken = default);

    ValueTask AddAsync(RewardItem rewardItem, CancellationToken cancellationToken = default);

    void Update(RewardItem rewardItem);

    void Remove(RewardItem rewardItem);
}
