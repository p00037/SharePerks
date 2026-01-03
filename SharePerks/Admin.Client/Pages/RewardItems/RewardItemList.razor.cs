using Admin.Client.Models;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.Entities;

namespace Admin.Client.Pages.RewardItems;

public partial class RewardItemList
{
    [Inject] public IRewardItemApiClient ApiClient { get; set; } = default!;
    [Inject] public IDialogService DialogService { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await RunAsync(async () =>
        {
            var items = await ApiClient.ListAsync();
            InitializeEditContext(items);
        }, "優待商品一覧の取得に失敗しました。");
    }

    private async Task OnDeleteAsync(RewardItem item)
    {
        var confirmed = await DialogService.ShowMessageBox(
            "削除確認",
            $"優待商品『{item.ItemName}』を削除してもよろしいですか？",
            yesText: "削除",
            cancelText: "キャンセル");

        if (confirmed != true)
        {
            return;
        }

        await RunAsync(async () =>
        {
            await ApiClient.DeleteAsync(item.ItemId);
            _formModel.Remove(item);
            Snackbar.Add($"優待商品『{item.ItemName}』を削除しました。", Severity.Success);
        }, "優待商品の削除に失敗しました。");
    }
}
