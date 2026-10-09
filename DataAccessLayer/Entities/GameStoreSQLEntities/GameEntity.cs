using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class GameEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Required]
    [MaxLength(250)]
    public string Key { get; set; }

    [MaxLength(3000)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public double Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int UnitInStock { get; set; }

    [Required]
    [Range(0, 100)]
    public int Discount { get; set; }

    [Required]
    public Guid PublisherId { get; set; }

    public int Views { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PublisherEntity Publisher { get; set; }

    public ICollection<GameGenreEntity> GameGenres { get; set; }

    public ICollection<GamePlatformEntity> GamePlatforms { get; set; }

    public GameFileEntity GameFile { get; set; }

    public ICollection<CommentEntity> Comments { get; set; }
}
