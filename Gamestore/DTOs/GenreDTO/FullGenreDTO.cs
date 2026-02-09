using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.GenreDTO;

public class FullGenreDTO : GenreNameAndParentGenreIdDto
{
    [Required(ErrorMessage = "Genre ID is required")]
    public Guid? Id { get; set; }
}
