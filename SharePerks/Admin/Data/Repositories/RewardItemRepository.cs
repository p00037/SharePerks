using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Entities;

namespace Admin.Data.Repositories;

public class RewardItemRepository : GenericRepository<RewardItem>, IRewardItemRepository
{
    public RewardItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RewardItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ItemId == id, cancellationToken);
    }

    public Task<bool> ExistsByItemCodeAsync(string itemCode, CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(x => x.ItemCode == itemCode, cancellationToken);
    }

    public override ValueTask<EntityEntry<RewardItem>> AddAsync(RewardItem rewardItem, CancellationToken cancellationToken = default)
    {
        return base.AddAsync(rewardItem, cancellationToken);
    }

    public override void Update(RewardItem rewardItem)
    {
        base.Update(rewardItem);
    }

    public override void Remove(RewardItem rewardItem)
    {
        base.Remove(rewardItem);
    }
}
