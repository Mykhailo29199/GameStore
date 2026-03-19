namespace Gamestore.WebAPI.DTOs.GameDTO;

public class FilteredGamesResponseDTO
{
    public IEnumerable<GameWithIdDTO> Games { get; set; }

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; }
}
