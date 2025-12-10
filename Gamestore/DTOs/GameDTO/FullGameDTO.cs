namespace Gamestore.WebAPI.DTOs.GameDTO;

public class FullGameDTO
{
    public GameDTO Game { get; set; }

    public List<Guid>? Genres { get; set; } = [];

    public List<Guid>? Platforms { get; set; } = [];
}