namespace Gamestore.WebAPI.DTOs.GameDTO;

public class FullGameDto
{
    public GameDto Game { get; set; }

    public List<Guid> Genres { get; set; }

    public List<Guid> Platforms { get; set; }
}