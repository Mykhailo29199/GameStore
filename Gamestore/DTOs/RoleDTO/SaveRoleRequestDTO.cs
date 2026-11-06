using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.RoleDTO;

/// <summary>
/// Request of the add role and update role endpoints. The role id is used only for the update.
/// </summary>
public class SaveRoleRequestDTO
{
    [Required]
    public RoleDTO Role { get; set; }

    public List<string> Permissions { get; set; } = [];
}
