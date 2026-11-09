using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities.Localization;

/// <summary>
/// A company name is not translated, so only the description has language versions.
/// </summary>
public class SupplierTranslationEntity : TranslationBaseEntity
{
    public Guid SupplierId { get; set; }

    [MaxLength(3000)]
    public string? Description { get; set; }

    public SupplierEntity Supplier { get; set; }
}
