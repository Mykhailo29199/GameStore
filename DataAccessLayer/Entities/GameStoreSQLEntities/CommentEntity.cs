using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

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
    public Guid ProductId { get; set; }

    public ProductEntity Product { get; set; }
}
