using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class GenreEntity
{
    [Key]
    [Required]
    public Guid GenreId { get; set; }

    [Required]
    [MaxLength(200)]
    public string GenreName { get; set; }

    public Guid? ParentGenreId { get; set; }

    public GenreEntity? ParentGenre { get; set; }

    public ICollection<GenreEntity>? SubGenres { get; set; } = new List<GenreEntity>();
}
