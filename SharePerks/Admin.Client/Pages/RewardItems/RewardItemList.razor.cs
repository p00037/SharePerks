using Admin.Client.Models;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components;
using Shared.Entities;

namespace Admin.Client.Pages.RewardItems;

public partial class RewardItemList
{
    [Inject] public IRewardItemApiClient ApiClient { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await RunAsync(async () =>
        {
            var items = await ApiClient.ListAsync();
            InitializeEditContext(items);
        }, "優待商品一覧の取得に失敗しました。");
    }
}
