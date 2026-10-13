using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class PlatformEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Type { get; set; }

    public ICollection<ProductPlatformEntity> ProductPlatforms { get; set; } = new List<ProductPlatformEntity>();
}
