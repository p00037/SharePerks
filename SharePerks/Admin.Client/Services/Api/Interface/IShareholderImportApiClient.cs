using Microsoft.AspNetCore.Components.Forms;
using Shared.Entities;

namespace Admin.Client.Services.Api.Interface;

public interface IShareholderImportApiClient
{
    Task<ImportBatch> ImportAsync(IBrowserFile file, CancellationToken cancellationToken = default);
}
