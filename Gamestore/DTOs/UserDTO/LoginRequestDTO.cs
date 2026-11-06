using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.UserDTO;

public class LoginRequestDTO
{
    [Required]
    public LoginDTO Model { get; set; }
}
