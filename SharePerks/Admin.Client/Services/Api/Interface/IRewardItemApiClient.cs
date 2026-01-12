using Admin.Client.Models;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Entities;

namespace Admin.Client.Services.Api.Interface
{
    public interface IRewardItemApiClient
    {
        Task<List<RewardItem>> ListAsync(CancellationToken cancellationToken = default);
        Task<RewardItem> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<RewardItem> CreateAsync(
            CreateRewardItemInput input,
            IBrowserFile? imageFile = null,
            CancellationToken cancellationToken = default);
        Task<RewardItem> UpdateAsync(
            int id,
            CreateRewardItemInput input,
            IBrowserFile? imageFile = null,
            CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
