namespace Gamestore.WebAPI.DTOs.GameDTO;

public class FullGameWithIdDto
{
    public GameWithIdDto GameWithIdDTO { get; set; }

    public List<Guid> Genres { get; set; }

    public List<Guid> Platforms { get; set; }
}
