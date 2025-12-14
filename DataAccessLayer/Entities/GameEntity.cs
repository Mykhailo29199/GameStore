using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class GameEntity
{
    [Key]
    [Required]
    public Guid GameId { get; set; }

    [Required]
    [MaxLength(200)]
    public string GameName { get; set; }

    [Required]
    [MaxLength(250)]
    public string GameKey { get; set; }

    [MaxLength(1000)]
    public string? GameDescription { get; set; }

    public ICollection<GameGenreEntity> GameGenres { get; set; }

    public ICollection<GamePlatformEntity> GamePlatforms { get; set; }
}
