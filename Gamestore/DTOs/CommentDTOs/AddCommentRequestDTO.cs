using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.CommentDTOs;

public class AddCommentRequestDTO
{
    [Required]
    public CommentContentDTO Comment { get; set; }

    public Guid? ParentId { get; set; }

    public string? Action { get; set; } // reply | quote
}
