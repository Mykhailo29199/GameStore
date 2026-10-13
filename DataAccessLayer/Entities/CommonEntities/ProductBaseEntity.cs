using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.CommonEntities;

public abstract class ProductBaseEntity
{
    public int? ProductID { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; }

    [Required]
    [MaxLength(250)]
    public string Key { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public double UnitPrice { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int UnitsInStock { get; set; }

    public int UnitsOnOrder { get; set; }

    public int ReorderLevel { get; set; }

    [MaxLength(100)]
    public string? QuantityPerUnit { get; set; }

    public int? Discontinued { get; set; }
}