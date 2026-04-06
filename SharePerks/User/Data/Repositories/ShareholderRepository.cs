using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace User.Data.Repositories;

public sealed class ShareholderRepository(ApplicationDbContext dbContext) : IShareholderRepository
{
    private readonly ApplicationDbContext dbContext = dbContext;

    public Task<Shareholder?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Shareholder>()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId && x.IsActive, cancellationToken);
    }

    public Task<Shareholder?> GetByUserIdForUpdateAsync(string userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Shareholder>()
            .SingleOrDefaultAsync(x => x.UserId == userId && x.IsActive, cancellationToken);
    }
}
