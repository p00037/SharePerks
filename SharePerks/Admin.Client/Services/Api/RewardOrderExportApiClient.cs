using System.Text.Json;
using Admin.Client.Services.Api.Interface;

namespace Admin.Client.Services.Api;

public sealed class RewardOrderExportApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IRewardOrderExportApiClient
{
    public async Task<RewardOrderExportDownloadResult> ExportAsync(string scope, CancellationToken cancellationToken = default)
    {
        using var response = await HttpClient.GetAsync($"api/admin/orders/export?scope={Uri.EscapeDataString(scope)}", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? "RewardOrders_Export.csv";
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "text/csv";
            return new RewardOrderExportDownloadResult(content, contentType, fileName);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = TryGetProblemDetail(body) ?? "申込CSVの出力に失敗しました。";
        throw new InvalidOperationException(message);
    }

    private static string? TryGetProblemDetail(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("detail", out var detailElement)
                && detailElement.ValueKind == JsonValueKind.String)
            {
                return detailElement.GetString();
            }

            if (document.RootElement.TryGetProperty("title", out var titleElement)
                && titleElement.ValueKind == JsonValueKind.String)
            {
                return titleElement.GetString();
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }
}
