namespace Admin.Client.Models;

public class RewardItemResponse
{
    public int ItemId { get; set; }

    public string ItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public string? ItemDescription { get; set; }

    public int RequiredPoints { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
