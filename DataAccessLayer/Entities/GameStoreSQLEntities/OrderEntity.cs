using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Entities.CommonEntities;
using GameStore.Shared.Enums;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class OrderEntity : OrderBaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public OrderStatus Status { get; set; }

    public Guid? ShipperId { get; set; }

    public ICollection<OrderDetailsEntity> OrderDetails { get; set; } = new List<OrderDetailsEntity>();

    public ShipperEntity Shipper { get; set; }

    [MaxLength(300)]
    public string? ShipAddress { get; set; }

    public string? ShipCity { get; set; }
}
