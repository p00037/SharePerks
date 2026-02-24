using Shareholder.Data.Repositories;

namespace Shareholder.Data;

public interface IUnitOfWork
{
    IRewardItemRepository RewardItems { get; }
    IShareholderRepository Shareholders { get; }
    IRewardOrderRepository RewardOrders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
