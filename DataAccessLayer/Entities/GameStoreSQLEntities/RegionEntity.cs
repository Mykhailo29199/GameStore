using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Entities.CommonEntities;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class RegionEntity : RegionBaseEntity
{
    [Key]
    public Guid? Id { get; set; } = Guid.NewGuid();
}