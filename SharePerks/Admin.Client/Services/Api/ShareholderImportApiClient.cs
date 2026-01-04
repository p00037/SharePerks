using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Admin.Client.Models;
using Admin.Client.Services.Api.Exceptions;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Entities;

namespace Admin.Client.Services.Api;

public class ShareholderImportApiClient : ApiClientBase, IShareholderImportApiClient
{
    private const long MaxFileSize = 20 * 1024 * 1024;

    public ShareholderImportApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<ImportBatch> ImportAsync(IBrowserFile file, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        await using var fileStream = file.OpenReadStream(MaxFileSize, cancellationToken);
        using var fileContent = new StreamContent(fileStream);

        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(file.ContentType) ? "text/csv" : file.ContentType);
        content.Add(fileContent, "file", file.Name);

        using var response = await HttpClient.PostAsync("api/admin/shareholders/import", content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ImportBatch>(cancellationToken: cancellationToken);
            return result ?? throw new InvalidOperationException("レスポンスの読み込みに失敗しました。");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>(
                cancellationToken: cancellationToken);

            if (problem is not null && problem.Errors is not null)
            {
                throw new ApiValidationException(
                    "入力内容を確認してください。",
                    new ReadOnlyDictionary<string, string[]>(problem.Errors));
            }
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"株主CSVインポートに失敗しました。({(int)response.StatusCode}) {body}", null, response.StatusCode);
    }
}
