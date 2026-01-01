using Admin.Client.Models;
using Admin.Client.Services.Api.Interface;
using Shared.Entities;

namespace Admin.Client.Services.Api;

public class RewardItemApiClient: ApiClientBase, IRewardItemApiClient
{
    public RewardItemApiClient(HttpClient httpClient):base(httpClient)
    {
    }

    public Task<List<RewardItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<List<RewardItem>>(
            "api/admin/items",
            failedMessage: "優待商品一覧の取得に失敗しました。",
            cancellationToken: cancellationToken);
    }

    public Task<RewardItem> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return GetAsync<RewardItem>(
            $"api/admin/items/{id}",
            failedMessage: "優待商品の取得に失敗しました。",
            cancellationToken: cancellationToken);
    }

    public async Task<RewardItem> CreateAsync(CreateRewardItemInput input, CancellationToken cancellationToken = default)
    {
        return await PostAsync<CreateRewardItemInput, RewardItem>(
            "api/admin/items",
            input,
            validationMessage: "入力内容を確認してください。",
            failedMessage: "優待商品の登録に失敗しました。",
            cancellationToken: cancellationToken);
    }

    public async Task<RewardItem> UpdateAsync(int id, CreateRewardItemInput input, CancellationToken cancellationToken = default)
    {
        return await PutAsync<CreateRewardItemInput, RewardItem>(
            $"api/admin/items/{id}",
            input,
            validationMessage: "入力内容を確認してください。",
            failedMessage: "優待商品の更新に失敗しました。",
            cancellationToken: cancellationToken);
    }
}
