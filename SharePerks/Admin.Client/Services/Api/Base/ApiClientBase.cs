using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Json;
using Admin.Client.Models;
using Admin.Client.Services.Api.Exceptions;

namespace Admin.Client.Services;

public abstract class ApiClientBase
{
    protected ApiClientBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected HttpClient HttpClient { get; }

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

        // 400: ValidationProblemDetails を優先して扱う
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>(
                cancellationToken: cancellationToken);

            if (problem is not null && problem.Errors is not null)
            {
                throw new ApiValidationException(
                    validationMessage ?? "入力内容に問題があります。",
                    new ReadOnlyDictionary<string, string[]>(problem.Errors));
            }
        }

        // 401/403 なども必要ならここで分岐して専用例外にしてOK
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var msg = failedMessage ?? "API呼び出しに失敗しました。";
        throw new HttpRequestException($"{msg}({(int)response.StatusCode}) {body}", null, response.StatusCode);
    }

    protected async Task<TResponse> PutAsync<TRequest, TResponse>(
        string url,
        TRequest request,
        string? validationMessage = null,
        string? failedMessage = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await HttpClient.PutAsJsonAsync(url, request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            return result ?? throw new InvalidOperationException("レスポンスの読み込みに失敗しました。");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetailsResponse>(
                cancellationToken: cancellationToken);

            if (problem is not null && problem.Errors is not null)
            {
                throw new ApiValidationException(
                    validationMessage ?? "入力内容に問題があります。",
                    new ReadOnlyDictionary<string, string[]>(problem.Errors));
            }
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var msg = failedMessage ?? "API呼び出しに失敗しました。";
        throw new HttpRequestException($"{msg}({(int)response.StatusCode}) {body}", null, response.StatusCode);
    }

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
        var msg = failedMessage ?? "API呼び出しに失敗しました。";
        throw new HttpRequestException($"{msg}({(int)response.StatusCode}) {body}", null, response.StatusCode);
    }

    protected async Task DeleteAsync(
        string url,
        string? failedMessage = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await HttpClient.DeleteAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var msg = failedMessage ?? "API呼び出しに失敗しました。";
        throw new HttpRequestException($"{msg}({(int)response.StatusCode}) {body}", null, response.StatusCode);
    }
}
