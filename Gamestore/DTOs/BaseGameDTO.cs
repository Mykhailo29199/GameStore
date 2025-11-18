using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs;

public class BaseGameDTO
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string? Description { get; set; }
}
