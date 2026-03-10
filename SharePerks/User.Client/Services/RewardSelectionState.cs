namespace User.Client.Services;

public sealed class RewardSelectionState
{
    public IReadOnlyList<SelectedRewardItem> SelectedItems { get; private set; } = Array.Empty<SelectedRewardItem>();

    public AddressInput Address { get; private set; } = new();

    public int TotalPoints => SelectedItems.Sum(item => item.RequiredPoints * item.Quantity);

    public void SetSelection(IEnumerable<SelectedRewardItem> items)
    {
        SelectedItems = items.ToList();
    }

    public void SetAddress(AddressInput address)
    {
        Address = address;
    }

    public void Clear()
    {
        SelectedItems = Array.Empty<SelectedRewardItem>();
        Address = new AddressInput();
    }
}

public sealed record SelectedRewardItem(
    int ItemId,
    string ItemName,
    string? ImagePath,
    int RequiredPoints,
    int Quantity
);

public sealed record AddressInput
{
    public string PostalCode { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Address1 { get; init; } = string.Empty;
    public string Address2 { get; init; } = string.Empty;
    public string? Address3 { get; init; }
}
