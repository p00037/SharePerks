using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Entities;

namespace Admin.Data.Repositories;

public interface IImportBatchRepository
{
    ValueTask<EntityEntry<ImportBatch>> AddAsync(ImportBatch importBatch, CancellationToken cancellationToken = default);
}
