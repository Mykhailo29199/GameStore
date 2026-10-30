using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Entities.CommonEntities;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class CategoryEntity : CategoryBaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? ParentCategoryId { get; set; }

    public bool IsDeleted { get; set; }

    public CategoryEntity? ParentGenre { get; set; }

    public ICollection<CategoryEntity>? SubGenres { get; set; } = new List<CategoryEntity>();

    public ICollection<ProductCategoryEntity> ProductCategories { get; set; } = new List<ProductCategoryEntity>();
}
