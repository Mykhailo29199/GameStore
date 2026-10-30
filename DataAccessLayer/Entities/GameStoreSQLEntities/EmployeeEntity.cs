using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccessLayer.Entities.CommonEntities;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class EmployeeEntity : EmployeeBaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? ReportsToId { get; set; }

    [ForeignKey("ReportsToId")]
    public EmployeeEntity? Manager { get; set; }

    public virtual ICollection<EmployeeTerritoryEntity> EmployeeTerritories { get; set; } = new List<EmployeeTerritoryEntity>();

    public virtual ICollection<EmployeeEntity> Subordinates { get; set; } = new List<EmployeeEntity>();
}