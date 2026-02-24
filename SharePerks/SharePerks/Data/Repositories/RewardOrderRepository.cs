using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace Shareholder.Data.Repositories;

public sealed class RewardOrderRepository(ApplicationDbContext dbContext) : IRewardOrderRepository
{
    private readonly ApplicationDbContext dbContext = dbContext;

    public Task<bool> ExistsByShareholderIdAsync(int shareholderId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<RewardOrder>()
            .AsNoTracking()
            .AnyAsync(x => x.ShareholderId == shareholderId && !x.IsCancelled, cancellationToken);
    }

    public void Add(RewardOrder rewardOrder)
    {
        dbContext.Set<RewardOrder>().Add(rewardOrder);
    }
}
