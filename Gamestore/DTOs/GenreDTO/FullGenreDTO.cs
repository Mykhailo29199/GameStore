using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.GenreDTO;

public class FullGenreDTO : GenreNameAndParentGenreIdDTO
{
    [Required(ErrorMessage = "Genre ID is required")]
    public Guid? Id { get; set; }
}
