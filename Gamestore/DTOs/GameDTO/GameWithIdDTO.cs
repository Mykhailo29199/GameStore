using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.GameDTO;

public class GameWithIdDto : GameDTO
{
    [Required(ErrorMessage = "Game ID is required")]
    public Guid? Id { get; set; }
}
