using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Entities;

[Table("M_Shareholder")]
public class Shareholder
{
    [Key]
    public int ShareholderId { get; set; }

    [Required]
    public required string UserId { get; set; }

    [Required]
    public required string ShareholderNo { get; set; }

    [Required]
    public required string ShareholderName { get; set; }

    public string? ShareholderNameKana { get; set; }

    [Required]
    public required string PostalCode { get; set; }

    [Required]
    public required string Address1 { get; set; }

    [Required]
    public required string Address2 { get; set; }

    public string? Address3 { get; set; }

    public string? PhoneNumber { get; set; }

    public int Holdings { get; set; }

    public string? CourseCode { get; set; }

    public int GrantedPoints { get; set; }

    public int UsedPoints { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<RewardOrder> Orders { get; set; } = new List<RewardOrder>();
}
