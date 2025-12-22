using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http.Json;
using Admin.Client.Models;
using Admin.Client.Services.Api.Interface;
using Shared.Entities;

namespace Admin.Client.Services.Api;

public class RewardItemApiClient: ApiClientBase, IRewardItemApiClient
{
    public RewardItemApiClient(HttpClient httpClient):base(httpClient)
    {
    }

    public async Task<RewardItem> CreateAsync(CreateRewardItemInput input, CancellationToken cancellationToken = default)
    {
        return await PostAsync<CreateRewardItemInput, RewardItem>(
            "api/admin/items",
            input);
    }
}
