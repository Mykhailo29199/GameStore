using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class OrderDetailsEntity
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid ProductId { get; set; } // gameId

    [Required]
    public double UnitPrice { get; set; }

    [Required]
    public int Quantity { get; set; }

    public int? Discount { get; set; }

    public OrderEntity Order { get; set; }

    public ProductEntity Product { get; set; }
}
