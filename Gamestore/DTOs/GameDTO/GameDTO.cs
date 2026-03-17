using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.GameDTO;

public class GameDTO
{
    [Required(ErrorMessage = "Game name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Game name must be between 1 and 200 characters.")]
    public string Name { get; set; }

    public string? Key { get; set; }

    public string? Description { get; set; }

    public double Price { get; set; }

    public int UnitInStock { get; set; }

    public int Discount { get; set; }
}
