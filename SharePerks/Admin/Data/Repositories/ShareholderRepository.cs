using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Entities;

namespace Admin.Data.Repositories;

public class ShareholderRepository : GenericRepository<Shareholder>, IShareholderRepository
{
    public ShareholderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<bool> ExistsByShareholderNoAsync(string shareholderNo, CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking().AnyAsync(x => x.ShareholderNo == shareholderNo, cancellationToken);
    }

    public override ValueTask<EntityEntry<Shareholder>> AddAsync(Shareholder shareholder, CancellationToken cancellationToken = default)
    {
        return base.AddAsync(shareholder, cancellationToken);
    }
}
