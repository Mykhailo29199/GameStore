using System.ComponentModel.DataAnnotations;
using DataAccessLayer.Entities.CommonEntities;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class TerritoryEntity : TerritoryBaseEntity
{
    [Key]
    public Guid? Id { get; set; } = Guid.NewGuid();

    public Guid? RegionGuidId { get; set; }

    public RegionEntity? RegionId { get; set; }

    public virtual ICollection<EmployeeTerritoryEntity>? EmployeeTerritories { get; set; } = new List<EmployeeTerritoryEntity>();
}