using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class OrderGameEntity
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid ProductId { get; set; } // gameId

    [Required]
    public double Price { get; set; }

    [Required]
    public int Quantity { get; set; }

    public int? Discount { get; set; }

    public OrderEntity Order { get; set; }

    public GameEntity Game { get; set; }
}
