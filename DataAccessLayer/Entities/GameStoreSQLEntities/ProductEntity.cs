using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class ProductEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; }

    [Required]
    [MaxLength(250)]
    public string Key { get; set; }

    [MaxLength(3000)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public double UnitPrice { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int UnitsInStock { get; set; }

    [Required]
    [Range(0, 100)]
    public int Discount { get; set; }

    [Required]
    public Guid SupplierID { get; set; }

    public int Views { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SupplierEntity Supplier { get; set; }

    public ICollection<ProductCategoryEntity> ProductCategories { get; set; }

    public ICollection<ProductPlatformEntity> ProductPlatforms { get; set; }

    public ProductFileEntity ProductFile { get; set; }

    public ICollection<CommentEntity> Comments { get; set; }
}
