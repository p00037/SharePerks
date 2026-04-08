using Shared.Entities;

namespace Admin.Data.Repositories;

public interface IRewardOrderRepository
{
    Task<List<RewardOrder>> ListForExportAsync(bool onlyUnexported, CancellationToken cancellationToken = default);
}
