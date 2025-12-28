using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components;
using Shared.Entities;

namespace Admin.Client.Pages.RewardItems;

public partial class RewardItemList
{
    [Inject] public IRewardItemApiClient ApiClient { get; set; } = default!;

    private List<RewardItem>? _items;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var items = await ApiClient.ListAsync();
            _items = items.ToList();
        }
        catch (Exception ex)
        {
            _errorMessage = "優待商品一覧の取得に失敗しました。";
            Console.Error.WriteLine(ex);
        }
    }
}
