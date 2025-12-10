using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.GameDTO;

public class GameWithIdDTO : GameDTO
{
    [Required]
    public Guid Id { get; set; }
}
