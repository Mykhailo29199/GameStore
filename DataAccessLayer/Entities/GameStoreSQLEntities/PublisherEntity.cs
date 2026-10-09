using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

[Table("PublisherEntity")]
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
