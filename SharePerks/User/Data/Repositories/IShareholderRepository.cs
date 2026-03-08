using Shared.Entities;

namespace User.Data.Repositories;

public interface IShareholderRepository
{
    Task<Shareholder?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
