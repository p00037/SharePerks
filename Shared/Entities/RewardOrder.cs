using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Entities;

[Table("T_RewardOrder")]
public class RewardOrder
{
    [Key]
    public int OrderId { get; set; }

    [ForeignKey(nameof(Shareholder))]
    public int ShareholderId { get; set; }

    [Required]
    public required string PostalCode { get; set; }

    [Required]
    public required string Address1 { get; set; }

    [Required]
    public required string Address2 { get; set; }

    public string? Address3 { get; set; }

    public string? PhoneNumber { get; set; }

    public int TotalPoints { get; set; }

    public DateTime OrderedAt { get; set; }

    public bool IsCancelled { get; set; }

    public bool IsExported { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Shareholder? Shareholder { get; set; }

    public ICollection<RewardOrderItem> OrderItems { get; set; } = new List<RewardOrderItem>();
}
