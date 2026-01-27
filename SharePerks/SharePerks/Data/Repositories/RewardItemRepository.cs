using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace Shareholder.Data.Repositories;

public sealed class RewardItemRepository(ApplicationDbContext dbContext) : IRewardItemRepository
{
    private readonly ApplicationDbContext dbContext = dbContext;

    public async Task<IReadOnlyList<RewardItem>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.RewardItems
            .AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.ItemId)
            .ToListAsync(cancellationToken);
    }
}
