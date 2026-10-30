using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class ProductPlatformEntity
{
    [Required]
    public Guid ProductId { get; set; }

    public ProductEntity Product { get; set; }

    [Required]
    public Guid PlatformId { get; set; }

    public PlatformEntity Platform { get; set; }
}