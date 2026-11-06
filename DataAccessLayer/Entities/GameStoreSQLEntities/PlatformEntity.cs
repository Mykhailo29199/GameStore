using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Entities.GameStoreSQLEntities.Localization;

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

    public ICollection<PlatformTranslationEntity> Translations { get; set; } = new List<PlatformTranslationEntity>();
}
