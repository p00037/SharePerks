using System.ComponentModel.DataAnnotations;

namespace Admin.Client.Models;

public class CreateRewardItemInput
{
    [Required]
    [MaxLength(100)]
    public string ItemCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? ItemDescription { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "1以上の数値を入力してください。")]
    public int RequiredPoints { get; set; } = 1;

    [Range(0, int.MaxValue, ErrorMessage = "0以上の数値を入力してください。")]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
