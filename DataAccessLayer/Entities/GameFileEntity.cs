using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class GameFileEntity
{
    [Key]
    [Required]
    public Guid GameId { get; set; }

    [Required]
    public string FilePath { get; set; }

    public GameEntity? Game { get; set; }
}
