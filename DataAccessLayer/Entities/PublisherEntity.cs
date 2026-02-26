using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class PublisherEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; }

    public string? HomePage { get; set; }

    public string? Description { get; set; }

    public ICollection<GameEntity> Games { get; set; } = new List<GameEntity>();
}
