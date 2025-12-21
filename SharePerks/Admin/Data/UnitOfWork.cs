using System;
using Admin.Data.Repositories;

namespace Admin.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Lazy<IRewardItemRepository> _rewardItemRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _rewardItemRepository = new Lazy<IRewardItemRepository>(() => new RewardItemRepository(context));
    }

    public IRewardItemRepository RewardItems => _rewardItemRepository.Value;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
