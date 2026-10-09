using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

[Table("CommentEntity")]
public class CommentEntity
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Body { get; set; }

    public Guid? ParentCommentId { get; set; }

    [Required]
    public bool IsDeleted { get; set; } = false;

    [Required]
    public Guid GameId { get; set; }

    public GameEntity Game { get; set; }
}
