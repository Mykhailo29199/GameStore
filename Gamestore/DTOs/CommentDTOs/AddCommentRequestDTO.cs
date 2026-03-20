using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.CommentDTOs;

public class AddCommentRequestDTO
{
    [Required]
    public CommentContentDTO Comment { get; set; }

    public string? ParentId { get; set; }

    public string? Action { get; set; }
}
