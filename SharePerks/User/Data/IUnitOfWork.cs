using User.Data.Repositories;

namespace User.Data;

public interface IUnitOfWork
{
    IRewardItemRepository RewardItems { get; }
    IShareholderRepository Shareholders { get; }
    IRewardOrderRepository RewardOrders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
