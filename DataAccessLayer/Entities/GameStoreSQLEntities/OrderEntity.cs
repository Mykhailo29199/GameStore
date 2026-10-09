using System.ComponentModel.DataAnnotations;
using GameStore.Shared.Enums;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class OrderEntity
{
    [Required]
    public Guid Id { get; set; }

    public DateTime? OrderDate { get; set; }

    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public OrderStatus Status { get; set; }

    public ICollection<OrderDetailsEntity> OrderDetails { get; set; } = new List<OrderDetailsEntity>();
}