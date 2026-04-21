using Shared.Dtos;
using User.Client.Components;
using User.Client.Services;
using User.Client.Services.Api.Interface;

namespace User.Client.Pages;

public partial class Items : FormComponentBase<List<RewardItemSummaryDto>>
{
    private readonly Dictionary<int, int> _quantityByItemId = new();
    private const int AvailablePoints = 5000;

    private int TotalPoints => _formModel.Sum(item => item.RequiredPoints * GetQuantity(item));

    private IReadOnlyList<SelectedRewardItem> SelectedItems => _formModel
        .Select(item => new SelectedRewardItem(
            item.ItemId,
            item.ItemName,
            item.ImagePath,
            item.RequiredPoints,
            GetQuantity(item)))
        .Where(item => item.Quantity > 0)
        .ToList();

    private bool CanProceed => SelectionState.IsExported
        ? SelectionState.HasExistingOrder
        : TotalPoints > 0 && TotalPoints <= AvailablePoints;

    protected override async Task OnInitializedAsync()
    {
        await RunAsync(LoadAsync, "優待商品の取得に失敗しました。時間をおいて再度お試しください。");
    }

    private async Task LoadAsync()
    {
        var itemsTask = ItemApiClient.ListAsync();
        var currentOrderTask = SelectionState.HasLoadedExistingOrder
            ? Task.FromResult<ShareholderOrderDto?>(null)
            : OrderApiClient.GetCurrentAsync();

        await Task.WhenAll(itemsTask, currentOrderTask);

        var items = await itemsTask;
        InitializeEditContext(items.ToList());

        var currentOrder = await currentOrderTask;
        if (currentOrder is not null)
        {
            ApplyExistingOrder(currentOrder);
        }
        else if (!SelectionState.HasLoadedExistingOrder)
        {
            RestoreFromSelectionState();
            SelectionState.MarkOrderLoaded();
        }
        else
        {
            RestoreFromSelectionState();
        }
    }

    private bool IsSelected(RewardItemSummaryDto item)
    {
        return GetQuantity(item) > 0;
    }

    private string GetCardClass(RewardItemSummaryDto item)
    {
        return IsSelected(item) ? "reward-card selected" : "reward-card";
    }

    private string GetImagePath(RewardItemSummaryDto item)
    {
        return string.IsNullOrWhiteSpace(item.ImagePath) ? "images/reward-placeholder.svg" : item.ImagePath;
    }

    private int GetQuantity(RewardItemSummaryDto item)
    {
        return _quantityByItemId.TryGetValue(item.ItemId, out var quantity) ? quantity : 0;
    }

    private void UpdateQuantity(RewardItemSummaryDto item, int quantity)
    {
        if (quantity <= 0)
        {
            _quantityByItemId.Remove(item.ItemId);
            return;
        }

        _quantityByItemId[item.ItemId] = quantity;
    }

    private async Task HandleMoveToAddressAsync()
    {
        await RunAsync(MoveToAddressAsync);
    }

    private Task MoveToAddressAsync()
    {
        if (!CanProceed)
        {
            return Task.CompletedTask;
        }

        SelectionState.SetSelection(SelectedItems);
        NavigationManager.NavigateTo("/address");
        return Task.CompletedTask;
    }

    private void ApplyExistingOrder(ShareholderOrderDto currentOrder)
    {
        SelectionState.SetExistingOrder(currentOrder.OrderId, currentOrder.IsExported);

        foreach (var item in currentOrder.Items)
        {
            _quantityByItemId[item.ItemId] = item.Quantity;

            if (_formModel.All(x => x.ItemId != item.ItemId))
            {
                _formModel.Add(new RewardItemSummaryDto(
                    item.ItemId,
                    item.ItemCode,
                    item.ItemName,
                    item.ItemDescription,
                    item.RequiredPoints,
                    item.ImagePath));
            }
        }

        _formModel.Sort(static (x, y) => x.ItemId.CompareTo(y.ItemId));

        SelectionState.SetSelection(currentOrder.Items.Select(item => new SelectedRewardItem(
            item.ItemId,
            item.ItemName,
            item.ImagePath,
            item.RequiredPoints,
            item.Quantity)));

        SelectionState.SetAddress(new AddressInput
        {
            PostalCode = currentOrder.PostalCode,
            PhoneNumber = currentOrder.PhoneNumber ?? string.Empty,
            Address1 = currentOrder.Address1,
            Address2 = currentOrder.Address2,
            Address3 = currentOrder.Address3
        });
    }

    private void RestoreFromSelectionState()
    {
        foreach (var item in SelectionState.SelectedItems)
        {
            _quantityByItemId[item.ItemId] = item.Quantity;
        }
    }
}
