namespace Gamestore.WebAPI.DTOs.GameDTO;

public class CreateGameDTO
{
    public GameDTO Game { get; set; }

    public List<Guid> Genres { get; set; }

    public List<Guid> Platforms { get; set; }

    public Guid Publisher { get; set; }
}