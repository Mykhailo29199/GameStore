using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class CategoryEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public CategoryEntity? ParentGenre { get; set; }

    public ICollection<CategoryEntity>? SubGenres { get; set; }
}
