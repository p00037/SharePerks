using Shareholder.Data.Repositories;

namespace Shareholder.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext dbContext;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
        RewardItems = new RewardItemRepository(dbContext);
        Shareholders = new ShareholderRepository(dbContext);
        RewardOrders = new RewardOrderRepository(dbContext);
    }

    public IRewardItemRepository RewardItems { get; }
    public IShareholderRepository Shareholders { get; }
    public IRewardOrderRepository RewardOrders { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
