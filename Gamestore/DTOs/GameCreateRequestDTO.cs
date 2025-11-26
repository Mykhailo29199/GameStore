namespace Gamestore.WebAPI.DTOs;

public class GameCreateRequestDTO
{
    public BaseGameDTO Game { get; set; }

    public List<Guid>? Genres { get; set; } = [];

    public List<Guid>? Platforms { get; set; } = [];
}