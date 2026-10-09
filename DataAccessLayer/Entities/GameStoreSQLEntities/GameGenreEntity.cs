using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class GameGenreEntity
{
    [Required]
    public Guid GameId { get; set; }

    public GameEntity Game { get; set; } = null!;

    [Required]
    public Guid GenreId { get; set; }

    public GenreEntity Genre { get; set; } = null!;
}
