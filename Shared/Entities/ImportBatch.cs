using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Entities;

[Table("T_ImportBatch")]
public class ImportBatch
{
    [Key]
    public int ImportId { get; set; }

    public DateTime ExecutedAt { get; set; }

    public int SuccessCount { get; set; }

    public int ErrorCount { get; set; }

    [Required]
    public required string FileName { get; set; }

    [Required]
    public required string AdminUserId { get; set; }

    public string? Remarks { get; set; }
}
