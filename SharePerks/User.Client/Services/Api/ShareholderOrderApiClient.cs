using User.Client.Services.Api.Interface;
using Shared.Dtos;

namespace User.Client.Services.Api;

public sealed class ShareholderOrderApiClient(HttpClient httpClient) : ApiClientBase(httpClient), IShareholderOrderApiClient
{
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
