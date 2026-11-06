using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities.Localization;

public class CategoryTranslationEntity : TranslationBaseEntity
{
    public Guid CategoryId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    public CategoryEntity Category { get; set; }
}
