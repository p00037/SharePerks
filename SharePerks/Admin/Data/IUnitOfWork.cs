using Admin.Data.Repositories;

namespace Admin.Data;

public interface IUnitOfWork : IAsyncDisposable
{
    IRewardItemRepository RewardItems { get; }
    IShareholderRepository Shareholders { get; }
    IImportBatchRepository ImportBatches { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
