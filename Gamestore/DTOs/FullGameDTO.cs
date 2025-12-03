namespace Gamestore.WebAPI.DTOs;

public class FullGameDTO
{
    public BaseGameDTO Game { get; set; }

    public List<Guid>? Genres { get; set; } = [];

    public List<Guid>? Platforms { get; set; } = [];
}