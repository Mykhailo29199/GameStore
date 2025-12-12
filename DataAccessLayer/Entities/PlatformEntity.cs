using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class PlatformEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Type { get; set; }
}
