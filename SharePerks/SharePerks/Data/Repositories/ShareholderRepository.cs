using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace Shareholder.Data.Repositories;

public sealed class ShareholderRepository(ApplicationDbContext dbContext) : IShareholderRepository
{
    private readonly ApplicationDbContext dbContext = dbContext;

    public Task<Shareholder?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Shareholder>()
            .SingleOrDefaultAsync(x => x.UserId == userId && x.IsActive, cancellationToken);
    }

    public void Update(Shareholder shareholder)
    {
        dbContext.Set<Shareholder>().Update(shareholder);
    }
}
