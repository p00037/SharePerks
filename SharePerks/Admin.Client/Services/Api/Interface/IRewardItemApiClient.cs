using Admin.Client.Models;
using Shared.Entities;

namespace Admin.Client.Services.Api.Interface
{
    public interface IRewardItemApiClient
    {
        Task<RewardItem> CreateAsync(
            CreateRewardItemInput input,
            CancellationToken cancellationToken = default);
    }
}
