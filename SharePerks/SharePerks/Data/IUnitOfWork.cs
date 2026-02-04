using Shareholder.Data.Repositories;

namespace Shareholder.Data;

public interface IUnitOfWork
{
    IRewardItemRepository RewardItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
