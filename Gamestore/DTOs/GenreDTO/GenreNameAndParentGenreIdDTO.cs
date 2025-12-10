using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.GenreDTO;

public class GenreNameAndParentGenreIdDTO
{
    [Required(ErrorMessage = "Genre name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Genre name must be between 1 and 200 characters.")]
    public string Name { get; set; }

    public Guid? ParentGenreId { get; set; }
}
