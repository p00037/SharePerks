using Shared.Entities;

namespace Admin.Client.Models;

public sealed class RewardItemBulkEditRow
{
    public RewardItemBulkEditRow()
    {
    }

    public RewardItemBulkEditRow(RewardItem item, int rowIndex)
    {
        RowIndex = rowIndex;
        ItemId = item.ItemId;
        ItemCode = item.ItemCode;
        ItemName = item.ItemName;
        IsActive = item.IsActive;
        RequiredPoints = item.RequiredPoints;
        DisplayOrder = item.DisplayOrder;
        OriginalRequiredPoints = item.RequiredPoints;
        OriginalDisplayOrder = item.DisplayOrder;
    }

    public int RowIndex { get; set; }

    public int ItemId { get; set; }

    public string ItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int RequiredPoints { get; set; }

    public int DisplayOrder { get; set; }

    public int OriginalRequiredPoints { get; set; }

    public int OriginalDisplayOrder { get; set; }

    public bool IsChanged => RequiredPoints != OriginalRequiredPoints
        || DisplayOrder != OriginalDisplayOrder;
}
