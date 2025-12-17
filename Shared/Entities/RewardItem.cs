using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Entities;

[Table("M_RewardItem")]
public class RewardItem
{
    [Key]
    public int ItemId { get; set; }

    [Required]
    public required string ItemCode { get; set; }

    [Required]
    public required string ItemName { get; set; }

    public string? ItemDescription { get; set; }

    public int RequiredPoints { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<RewardOrderItem> OrderItems { get; set; } = new List<RewardOrderItem>();
}
