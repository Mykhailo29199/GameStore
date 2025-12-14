using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.PlatformDTO;

public class PlatformDto
{
    [Required(ErrorMessage = "Platform ID is required")]
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Platform type is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Platform type must be between 1 and 200 characters.")]
    public string Type { get; set; }
}
