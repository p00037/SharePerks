using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Entities;

[Table("T_RewardOrderItem")]
public class RewardOrderItem
{
    [Key]
    public int OrderItemId { get; set; }

    [ForeignKey(nameof(Order))]
    public int OrderId { get; set; }

    [ForeignKey(nameof(Item))]
    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public int SubtotalPoints { get; set; }

    public RewardOrder? Order { get; set; }

    public RewardItem? Item { get; set; }
}
