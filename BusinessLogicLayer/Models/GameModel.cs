namespace BusinessLogicLayer.Models;

public class GameModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string? Description { get; set; }

    public double Price { get; set; }

    public int UnitInStock { get; set; }

    public int Discount { get; set; }

    public List<Guid> Genres { get; set; } = [];

    public List<Guid> Platforms { get; set; } = [];

    public Guid Publisher { get; set; }
}
