namespace Gamestore.WebAPI.DTOs.GameDTO;

public class FilteredGamesResultDTO
{
    public IEnumerable<GameWithIdDTO> Games { get; set; }

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; }
}
