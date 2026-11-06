namespace Gamestore.WebAPI.DTOs.UserDTO;

public class AccessRequestDTO
{
    public string TargetPage { get; set; } = string.Empty;

    public string? TargetId { get; set; }
}
