namespace Gamestore.WebAPI.DTOs.GameDTO;

public class FullGameWithIdDTO
{
    public GameWithIdDTO GameWithIdDTO { get; set; }

    public List<Guid> Genres { get; set; } = [];

    public List<Guid> Platforms { get; set; } = [];
}
