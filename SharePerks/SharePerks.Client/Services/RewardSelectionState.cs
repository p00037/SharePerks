namespace Shareholder.Client.Services;

public sealed class RewardSelectionState
{
    public IReadOnlyList<SelectedRewardItem> SelectedItems { get; private set; } = Array.Empty<SelectedRewardItem>();

    public int TotalPoints => SelectedItems.Sum(item => item.RequiredPoints * item.Quantity);

    public void SetSelection(IEnumerable<SelectedRewardItem> items)
    {
        SelectedItems = items.ToList();
    }

    public void Clear()
    {
        SelectedItems = Array.Empty<SelectedRewardItem>();
    }
}

public sealed record SelectedRewardItem(
    int ItemId,
    string ItemName,
    string? ImagePath,
    int RequiredPoints,
    int Quantity
);
