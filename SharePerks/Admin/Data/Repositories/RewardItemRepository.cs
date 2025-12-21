using Microsoft.EntityFrameworkCore;
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

    public ValueTask AddAsync(RewardItem rewardItem, CancellationToken cancellationToken = default)
    {
        return base.AddAsync(rewardItem, cancellationToken);
    }

    public void Update(RewardItem rewardItem)
    {
        base.Update(rewardItem);
    }

    public void Remove(RewardItem rewardItem)
    {
        base.Remove(rewardItem);
    }
}
