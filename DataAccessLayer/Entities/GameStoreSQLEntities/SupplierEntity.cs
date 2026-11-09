using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Entities.CommonEntities;
using DataAccessLayer.Entities.GameStoreSQLEntities.Localization;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class SupplierEntity : SupplierBaseEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();

    public ICollection<SupplierTranslationEntity> Translations { get; set; } = new List<SupplierTranslationEntity>();
}
