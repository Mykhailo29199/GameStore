using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

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
    public Guid GameId { get; set; }

    public GameEntity Game { get; set; }
}
