using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs;

public class GameWithIdDTO : BaseGameDTO
{
    [Required]
    public Guid Id { get; set; }
}
