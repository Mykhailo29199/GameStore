using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccessLayer.Entities.CommonEntities;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

[Table("Products")]
public class ProductEntity : ProductBaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(3000)]
    public string? Description { get; set; }

    public double Discount { get; set; }

    public int Views { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid SupplierID { get; set; }

    public bool IsDeleted { get; set; }

    public SupplierEntity Supplier { get; set; }

    public ICollection<ProductCategoryEntity> ProductCategories { get; set; } = new List<ProductCategoryEntity>();

    public ICollection<ProductPlatformEntity> ProductPlatforms { get; set; } = new List<ProductPlatformEntity>();

    public ProductFileEntity ProductFile { get; set; }

    public ICollection<CommentEntity> Comments { get; set; } = new List<CommentEntity>();
}