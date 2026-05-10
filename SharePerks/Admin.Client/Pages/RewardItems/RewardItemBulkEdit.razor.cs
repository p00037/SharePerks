using Admin.Client.Components;
using Admin.Client.Models;
using Admin.Client.Services.Api.Interface;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Shared.Dtos;
using Shared.Entities;

namespace Admin.Client.Pages.RewardItems;

public partial class RewardItemBulkEdit : FormComponentBase<List<RewardItemBulkEditRow>>
{
    private const int RequiredPointsColumnIndex = 0;
    private const int DisplayOrderColumnIndex = 1;

    [Inject] public IRewardItemApiClient ApiClient { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;

    private int ChangedCount => _formModel.Count(row => row.IsChanged);

    private bool HasChanges => ChangedCount > 0;

    protected override async Task OnInitializedAsync()
    {
        InitializeEditContext(new List<RewardItemBulkEditRow>());
        await RunAsync(LoadAsync, "優待商品一覧の取得に失敗しました。時間をおいて再度お試しください。");
    }

    private async Task LoadAsync()
    {
        var items = await ApiClient.ListAsync();
        await ResetForm(ToRows(items));
    }

    private async Task HandleReloadAsync()
    {
        await RunAsync(LoadAsync, "優待商品一覧の再読み込みに失敗しました。");
    }

    private async Task HandleSaveAsync()
    {
        await RunAsync(SaveAsync, "優待商品の必要ポイント・表示順の一括更新に失敗しました。");
    }

    private async Task SaveAsync()
    {
        var changedRows = _formModel.Where(row => row.IsChanged).ToArray();
        if (changedRows.Length == 0)
        {
            return;
        }

        var request = new RewardItemBulkUpdateRequestDto(
            changedRows
                .Select(row => new RewardItemBulkUpdateRowDto(row.ItemId, row.RequiredPoints, row.DisplayOrder))
                .ToArray());

        var updatedItems = await ApiClient.BulkUpdateOrderPointsAsync(request);
        await ResetForm(ToRows(updatedItems));
        Snackbar.Add("必要ポイント・表示順を更新しました。", Severity.Success);
    }

    private void UpdateRequiredPoints(RewardItemBulkEditRow row, object? value)
    {
        row.RequiredPoints = ParseIntOrDefault(value, row.RequiredPoints);
    }

    private void UpdateDisplayOrder(RewardItemBulkEditRow row, object? value)
    {
        row.DisplayOrder = ParseIntOrDefault(value, row.DisplayOrder);
    }

    private string GetArrowKeyHandler(int rowIndex, int columnIndex)
    {
        return $"sharePerksBulkEdit.handleKeyDown(event,{rowIndex},{columnIndex},{_formModel.Count},2)";
    }

    private static string GetCellId(int rowIndex, int columnIndex)
    {
        return $"reward-item-bulk-edit-{rowIndex}-{columnIndex}";
    }

    private static int ParseIntOrDefault(object? value, int currentValue)
    {
        return int.TryParse(value?.ToString(), out var parsed) ? parsed : currentValue;
    }

    private static List<RewardItemBulkEditRow> ToRows(IReadOnlyList<RewardItem> items)
    {
        return items
            .Select((item, index) => new RewardItemBulkEditRow(item, index))
            .ToList();
    }
}
