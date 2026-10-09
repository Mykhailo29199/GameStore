using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class ProductCategoryEntity
{
    [Required]
    public Guid ProductId { get; set; }

    public ProductEntity Product { get; set; } = null!;

    [Required]
    public Guid CategoryId { get; set; }

    public CategoryEntity Category { get; set; } = null!;
}
