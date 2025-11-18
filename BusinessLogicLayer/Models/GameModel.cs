namespace BusinessLogicLayer.Models;

public class GameModel
{
    public BaseGameModel Game { get; set; }

    public List<Guid> Genres { get; set; } = [];

    public List<Guid> Platforms { get; set; } = [];
}
