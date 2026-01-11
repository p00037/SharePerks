using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace Admin.Client.Models;

public class ShareholderImportInput
{
    [Required(ErrorMessage = "CSVファイルを選択してください。")]
    public IBrowserFile? File { get; set; }
}
