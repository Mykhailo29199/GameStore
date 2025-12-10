using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.GenreDTO;

public class FullGenreDTO
{
    [Required(ErrorMessage = "Genre Id is required.")]
    public Guid Id { get; set; }

    public GenreNameAndParentGenreIdDTO Genre { get; set; }
}
