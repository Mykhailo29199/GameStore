using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class PlatformEntity
{
    [Key]
    [Required]
    public Guid PlatformId { get; set; }

    [Required]
    [MaxLength(200)]
    public string PlatformType { get; set; }
}
