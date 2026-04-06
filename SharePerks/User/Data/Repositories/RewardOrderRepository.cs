using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace User.Data.Repositories;

public sealed class RewardOrderRepository(ApplicationDbContext dbContext) : IRewardOrderRepository
{
    private readonly ApplicationDbContext dbContext = dbContext;

    public Task<bool> ExistsByShareholderIdAsync(int shareholderId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<RewardOrder>()
            .AsNoTracking()
            .AnyAsync(x => x.ShareholderId == shareholderId && !x.IsCancelled, cancellationToken);
    }

    public Task<RewardOrder?> GetByShareholderIdAsync(int shareholderId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<RewardOrder>()
            .AsNoTracking()
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Item)
            .SingleOrDefaultAsync(x => x.ShareholderId == shareholderId && !x.IsCancelled, cancellationToken);
    }

    public Task<RewardOrder?> GetByShareholderIdForUpdateAsync(int shareholderId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<RewardOrder>()
            .Include(x => x.OrderItems)
            .SingleOrDefaultAsync(x => x.ShareholderId == shareholderId && !x.IsCancelled, cancellationToken);
    }

    public void Add(RewardOrder rewardOrder)
    {
        dbContext.Set<RewardOrder>().Add(rewardOrder);
    }

    public void ReplaceItems(RewardOrder rewardOrder, IReadOnlyCollection<RewardOrderItem> orderItems)
    {
        dbContext.Set<RewardOrderItem>().RemoveRange(rewardOrder.OrderItems);
        rewardOrder.OrderItems.Clear();

        foreach (var orderItem in orderItems)
        {
            rewardOrder.OrderItems.Add(orderItem);
        }
    }
}
