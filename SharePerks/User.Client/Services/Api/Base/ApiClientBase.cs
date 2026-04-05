using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Json;
using User.Client.Models;
using User.Client.Services.Api.Exceptions;

namespace User.Client.Services.Api;

public abstract class ApiClientBase(HttpClient httpClient)
{
    protected HttpClient HttpClient { get; } = httpClient;

    protected async Task<TResponse> GetAsync<TResponse>(
        string url,
        string? failedMessage = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await HttpClient.GetAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            return result ?? throw new InvalidOperationException("レスポンスの読み込みに失敗しました。");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"{failedMessage ?? "API呼び出しに失敗しました。"}({(int)response.StatusCode}) {body}",
            null,
            response.StatusCode);
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(
        string url,
        TRequest request,
        string? validationMessage = null,
        string? failedMessage = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await HttpClient.PostAsJsonAsync(url, request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            return result ?? throw new InvalidOperationException("レスポンスの読み込みに失敗しました。");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>(
                cancellationToken: cancellationToken);

            if (problem?.Errors is not null)
            {
                throw new ApiValidationException(
                    validationMessage ?? "入力内容に問題があります。",
                    new ReadOnlyDictionary<string, string[]>(problem.Errors));
            }
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"{failedMessage ?? "API呼び出しに失敗しました。"}({(int)response.StatusCode}) {body}",
            null,
            response.StatusCode);
    }
}
