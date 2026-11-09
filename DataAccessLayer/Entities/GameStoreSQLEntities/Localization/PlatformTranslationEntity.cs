using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities.Localization;

public class PlatformTranslationEntity : TranslationBaseEntity
{
    public Guid PlatformId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Type { get; set; }

    public PlatformEntity Platform { get; set; }
}
