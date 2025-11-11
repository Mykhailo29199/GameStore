using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class GamePlatformEntity
{
    [Required]
    public Guid GameId { get; set; }

    public GameEntity Game { get; set; } = null!;

    [Required]
    public Guid PlatformId { get; set; }

    public PlatformEntity Platform { get; set; } = null!;
}
