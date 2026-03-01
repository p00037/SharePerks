using Shared.Dtos;

namespace Shareholder.Client.Services.Api.Interface;

public interface IShareholderOrderApiClient
{
    Task<CreateRewardOrderResponseDto> CreateAsync(
        CreateRewardOrderRequestDto request,
        CancellationToken cancellationToken = default);
}
