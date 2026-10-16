using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.UserDTO;

/// <summary>
/// Request of the add user and update user endpoints. The user id is used only for the update.
/// </summary>
public class SaveUserRequestDTO
{
    [Required]
    public UserDTO User { get; set; }

    public List<Guid> Roles { get; set; } = [];

    public string? Password { get; set; }
}
