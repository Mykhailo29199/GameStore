using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities.Localization;

public class ProductTranslationEntity : TranslationBaseEntity
{
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(3000)]
    public string? Description { get; set; }

    public ProductEntity Product { get; set; }
}
