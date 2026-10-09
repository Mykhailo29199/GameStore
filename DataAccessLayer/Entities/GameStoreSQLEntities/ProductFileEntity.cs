using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class ProductFileEntity
{
    [Key]
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public string FilePath { get; set; }

    public ProductEntity? Product { get; set; }
}
