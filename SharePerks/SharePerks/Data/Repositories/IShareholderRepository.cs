using Shared.Entities;

namespace Shareholder.Data.Repositories;

public interface IShareholderRepository
{
    Task<Shareholder?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    void Update(Shareholder shareholder);
}
