using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Entities;

namespace Admin.Data.Repositories;

public class ImportBatchRepository : GenericRepository<ImportBatch>, IImportBatchRepository
{
    public ImportBatchRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override ValueTask<EntityEntry<ImportBatch>> AddAsync(ImportBatch importBatch, CancellationToken cancellationToken = default)
    {
        return base.AddAsync(importBatch, cancellationToken);
    }
}
