namespace Gamestore.WebAPI.DTOs.GameDTO;

public class UpdateGameDTO
{
    public GameWithIdDto Game { get; set; }

    public Guid Publisher { get; set; }

    public List<Guid> Genres { get; set; }

    public List<Guid> Platforms { get; set; }
}
