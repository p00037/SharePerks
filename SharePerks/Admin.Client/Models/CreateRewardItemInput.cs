using System.ComponentModel.DataAnnotations;

namespace Admin.Client.Models;

public class CreateRewardItemInput
{
    [Display(Name = "商品コード")]
    [Required(ErrorMessage = "「{0}」は必須です。")]
    [MaxLength(100, ErrorMessage = "「{0}」は{1}文字以内で入力してください。")]
    public string ItemCode { get; set; } = string.Empty;

    [Display(Name = "商品名")]
    [Required(ErrorMessage = "「{0}」は必須です。")]
    [MaxLength(200, ErrorMessage = "「{0}」は{1}文字以内で入力してください。")]
    public string ItemName { get; set; } = string.Empty;

    [Display(Name = "商品説明")]
    [MaxLength(1000, ErrorMessage = "「{0}」は{1}文字以内で入力してください。")]
    public string? ItemDescription { get; set; }

    [Display(Name = "必要ポイント")]
    [Range(1, int.MaxValue, ErrorMessage = "1以上の数値を入力してください。")]
    public int RequiredPoints { get; set; } = 1;

    [Display(Name = "表示順")]
    [Range(0, int.MaxValue, ErrorMessage = "0以上の数値を入力してください。")]
    public int DisplayOrder { get; set; }

    [Display(Name = "公開する")]
    public bool IsActive { get; set; } = true;
}
