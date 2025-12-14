using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.GenreDTO;

public class FullGenreDto
{
    [Required(ErrorMessage = "Genre ID is required")]
    public Guid? Id { get; set; }

    public GenreNameAndParentGenreIdDto Genre { get; set; }
}
