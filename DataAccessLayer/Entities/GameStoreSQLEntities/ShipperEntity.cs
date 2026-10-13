using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Entities.CommonEntities;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class ShipperEntity : ShipperBaseEntity
{
    [Key]
    public Guid? Id { get; set; } = Guid.NewGuid();

    public ICollection<OrderEntity>? Orders { get; set; }
}