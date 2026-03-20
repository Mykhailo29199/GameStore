using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.Users;

public class BanRequestDTO
{
    [Required]
    public string User { get; set; }

    [Required]
    public string Duration { get; set; }
}
