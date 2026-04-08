using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace Admin.Data.Repositories;

public sealed class RewardOrderRepository(ApplicationDbContext dbContext) : IRewardOrderRepository
{
    private readonly ApplicationDbContext dbContext = dbContext;

    public Task<List<RewardOrder>> ListForExportAsync(bool onlyUnexported, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<RewardOrder>()
            .Include(x => x.Shareholder)
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Item)
            .Where(x => !x.IsCancelled);

        if (onlyUnexported)
        {
            query = query.Where(x => !x.IsExported);
        }

        return query
            .OrderBy(x => x.OrderedAt)
            .ThenBy(x => x.OrderId)
            .ToListAsync(cancellationToken);
    }
}
