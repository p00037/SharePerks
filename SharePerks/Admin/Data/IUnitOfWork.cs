using Admin.Data.Repositories;

namespace Admin.Data;

public interface IUnitOfWork : IAsyncDisposable
{
    IRewardItemRepository RewardItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
