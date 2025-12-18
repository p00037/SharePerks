using System.ComponentModel.DataAnnotations;

namespace Admin.Models;

public class CreateRewardItemRequest
{
    [Required]
    [MaxLength(100)]
    public string ItemCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? ItemDescription { get; set; }

    [Range(1, int.MaxValue)]
    public int RequiredPoints { get; set; }

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
