using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Entities;

namespace Admin.Data.Repositories;

public interface IRewardItemRepository
{
    Task<List<RewardItem>> ListAsync(CancellationToken cancellationToken = default);

    Task<RewardItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByItemCodeAsync(string itemCode, int? excludeItemId = null, CancellationToken cancellationToken = default);

    ValueTask<EntityEntry<RewardItem>> AddAsync(RewardItem rewardItem, CancellationToken cancellationToken = default);

    void Update(RewardItem rewardItem);

    void Remove(RewardItem rewardItem);
}
