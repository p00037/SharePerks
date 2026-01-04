using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Entities;

namespace Admin.Data.Repositories;

public interface IShareholderRepository
{
    Task<bool> ExistsByShareholderNoAsync(string shareholderNo, CancellationToken cancellationToken = default);
    ValueTask<EntityEntry<Shareholder>> AddAsync(Shareholder shareholder, CancellationToken cancellationToken = default);
}
