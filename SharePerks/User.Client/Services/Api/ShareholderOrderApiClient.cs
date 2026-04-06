using System.Net;
using System.Net.Http.Json;
using User.Client.Services.Api.Interface;
using Shared.Dtos;

namespace User.Client.Services.Api;

public sealed class ShareholderOrderApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IShareholderOrderApiClient
{
    public async Task<ShareholderOrderDto?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        using var response = await HttpClient.GetAsync("api/shareholder/order", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ShareholderOrderDto>(cancellationToken: cancellationToken);
            return result ?? throw new InvalidOperationException("レスポンスの読み込みに失敗しました。");
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"申込情報の取得に失敗しました。({(int)response.StatusCode}) {body}",
            null,
            response.StatusCode);
    }

    public Task<CreateRewardOrderResponseDto> CreateAsync(
        CreateRewardOrderRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateRewardOrderRequestDto, CreateRewardOrderResponseDto>(
            "api/shareholder/order",
            request,
            validationMessage: "入力内容を確認してください。",
            failedMessage: "申し込み登録に失敗しました。",
            cancellationToken: cancellationToken);
    }
}
