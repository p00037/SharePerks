using Shared.Dtos;

namespace User.Client.Services.Api.Interface;

public interface IShareholderOrderApiClient
{
    Task<ShareholderOrderDto?> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task<CreateRewardOrderResponseDto> CreateAsync(
        CreateRewardOrderRequestDto request,
        CancellationToken cancellationToken = default);
}
