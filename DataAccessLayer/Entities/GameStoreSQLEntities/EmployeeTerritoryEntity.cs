using System.ComponentModel.DataAnnotations.Schema;
using DataAccessLayer.Entities.CommonEntities;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class EmployeeTerritoryEntity : EmployeeTerritoryBaseEntity
{
    public Guid EmployeeGuidId { get; set; }

    public Guid TerritoryGuidId { get; set; }

    [ForeignKey("EmployeeGuidId")]
    public virtual EmployeeEntity Employee { get; set; }

    [ForeignKey("TerritoryGuidId")]
    public virtual TerritoryEntity Territory { get; set; }
}