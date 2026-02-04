using Shareholder.Data.Repositories;

namespace Shareholder.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext dbContext;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
        RewardItems = new RewardItemRepository(dbContext);
    }

    public IRewardItemRepository RewardItems { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
