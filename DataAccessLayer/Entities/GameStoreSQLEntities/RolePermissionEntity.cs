using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class RolePermissionEntity
{
    public Guid RoleId { get; set; }

    public RoleEntity Role { get; set; }

    [Required]
    [MaxLength(100)]
    public string Permission { get; set; }
}
