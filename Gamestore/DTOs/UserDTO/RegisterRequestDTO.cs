using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.UserDTO;

public class RegisterRequestDTO
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Login { get; set; } = string.Empty;

    [Required]
    [MinLength(4, ErrorMessage = "Password must contain at least 4 characters.")]
    public string Password { get; set; } = string.Empty;
}
