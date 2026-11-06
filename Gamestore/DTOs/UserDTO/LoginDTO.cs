namespace Gamestore.WebAPI.DTOs.UserDTO;

public class LoginDTO
{
    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool InternalAuth { get; set; }
}
