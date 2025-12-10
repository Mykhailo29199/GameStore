using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.GameDTO;

public class GameDTO
{
    [Required(ErrorMessage = "Game name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Game name must be between 1 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string? Description { get; set; }
}
