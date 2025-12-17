using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Entities;

[Table("M_SystemConfig")]
public class SystemConfig
{
    [Key]
    [MaxLength(100)]
    public required string ConfigKey { get; set; }

    [Required]
    public required string ConfigValue { get; set; }

    public DateTime UpdatedAt { get; set; }
}
