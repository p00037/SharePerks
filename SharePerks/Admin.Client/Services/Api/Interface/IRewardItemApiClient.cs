using Admin.Client.Models;
using Shared.Entities;

namespace Admin.Client.Services.Api.Interface
{
public interface IRewardItemApiClient
{
    Task<List<RewardItem>> ListAsync(CancellationToken cancellationToken = default);
    Task<RewardItem> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RewardItem> CreateAsync(
        CreateRewardItemInput input,
        CancellationToken cancellationToken = default);
    Task<RewardItem> UpdateAsync(
        int id,
        CreateRewardItemInput input,
        CancellationToken cancellationToken = default);
}
}
