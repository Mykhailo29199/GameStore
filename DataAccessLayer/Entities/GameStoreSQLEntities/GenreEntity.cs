using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class GenreEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    public Guid? ParentGenreId { get; set; }

    public GenreEntity? ParentGenre { get; set; }

    public ICollection<GenreEntity>? SubGenres { get; set; }
}
