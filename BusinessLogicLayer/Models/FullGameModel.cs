namespace BusinessLogicLayer.Models;

public class FullGameModel
{
    public GameModel Game { get; set; }

    public List<Guid> Genres { get; set; } = [];

    public List<Guid> Platforms { get; set; } = [];
}
