using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Entities.CommonEntities;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class OrderDetailsEntity : OrderDetailsBaseEntity
{
    /// <summary>
    /// Gets or sets the identifier of the order detail used by the order management endpoints.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    public OrderEntity? Order { get; set; }

    public ProductEntity? Product { get; set; }
}
