using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Entities.CommonEntities;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class OrderDetailsEntity : OrderDetailsBaseEntity
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    public OrderEntity? Order { get; set; }

    public ProductEntity? Product { get; set; }
}
