namespace Admin.Client.Services.Api.Interface;

public interface IRewardOrderExportApiClient
{
    Task<RewardOrderExportDownloadResult> ExportAsync(string scope, CancellationToken cancellationToken = default);
}

public sealed record RewardOrderExportDownloadResult(byte[] Content, string ContentType, string FileName);
