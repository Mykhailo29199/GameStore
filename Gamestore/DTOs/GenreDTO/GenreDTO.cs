using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.GenreDTO;

public class GenreDTO
{
    [Required(ErrorMessage = "Genre ID is required")]
    public Guid? Id { get; set; }

    public string Name { get; set; }
}
