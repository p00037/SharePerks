namespace User.Client.Services;

public sealed class RewardSelectionState
{
    public IReadOnlyList<SelectedRewardItem> SelectedItems { get; private set; } = Array.Empty<SelectedRewardItem>();

    public AddressInput Address { get; private set; } = new();

    public int? OrderId { get; private set; }

    public bool IsExported { get; private set; }

    public bool HasLoadedExistingOrder { get; private set; }

    public bool HasExistingOrder => OrderId.HasValue;

    public int TotalPoints => SelectedItems.Sum(item => item.RequiredPoints * item.Quantity);

    public void SetSelection(IEnumerable<SelectedRewardItem> items)
    {
        SelectedItems = items.ToList();
    }

    public void SetAddress(AddressInput address)
    {
        Address = address;
    }

    public void SetExistingOrder(int orderId, bool isExported)
    {
        OrderId = orderId;
        IsExported = isExported;
        HasLoadedExistingOrder = true;
    }

    public void MarkOrderLoaded()
    {
        HasLoadedExistingOrder = true;
    }

    public void Clear()
    {
        SelectedItems = Array.Empty<SelectedRewardItem>();
        Address = new AddressInput();
        OrderId = null;
        IsExported = false;
        HasLoadedExistingOrder = false;
    }
}

public sealed record SelectedRewardItem(
    int ItemId,
    string ItemName,
    string? ImagePath,
    int RequiredPoints,
    int Quantity
);

public sealed class AddressInput
{
    public string PostalCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address1 { get; set; } = string.Empty;
    public string Address2 { get; set; } = string.Empty;
    public string? Address3 { get; set; }
}
