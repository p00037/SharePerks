using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Json;
using Admin.Client.Models;

namespace Admin.Client.Services;

public class RewardItemApiClient
{
    private readonly HttpClient _httpClient;

    public RewardItemApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RewardItemResponse> CreateAsync(CreateRewardItemInput input, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/admin/items", input, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<RewardItemResponse>(cancellationToken: cancellationToken);
            if (created is null)
            {
                throw new InvalidOperationException("登録結果の読み込みに失敗しました。");
            }

            return created;
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>(cancellationToken: cancellationToken);
            if (problem is not null)
            {
                throw new RewardItemValidationException(problem.Title ?? "入力内容に問題があります。", new ReadOnlyDictionary<string, string[]>(problem.Errors));
            }
        }

        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"優待商品の登録に失敗しました。({(int)response.StatusCode}) {message}");
    }
}

public class RewardItemValidationException : Exception
{
    public RewardItemValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
