using System;
using Admin.Data.Repositories;

namespace Admin.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Lazy<IRewardItemRepository> _rewardItemRepository;
    private readonly Lazy<IRewardOrderRepository> _rewardOrderRepository;
    private readonly Lazy<IShareholderRepository> _shareholderRepository;
    private readonly Lazy<IImportBatchRepository> _importBatchRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _rewardItemRepository = new Lazy<IRewardItemRepository>(() => new RewardItemRepository(context));
        _rewardOrderRepository = new Lazy<IRewardOrderRepository>(() => new RewardOrderRepository(context));
        _shareholderRepository = new Lazy<IShareholderRepository>(() => new ShareholderRepository(context));
        _importBatchRepository = new Lazy<IImportBatchRepository>(() => new ImportBatchRepository(context));
    }

    public IRewardItemRepository RewardItems => _rewardItemRepository.Value;
    public IRewardOrderRepository RewardOrders => _rewardOrderRepository.Value;
    public IShareholderRepository Shareholders => _shareholderRepository.Value;
    public IImportBatchRepository ImportBatches => _importBatchRepository.Value;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
