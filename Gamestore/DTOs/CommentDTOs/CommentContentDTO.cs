using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.CommentDTOs;

public class CommentContentDTO
{
    /// <summary>
    /// Gets or sets the commenter name. It is ignored: the name of the authorized user is used instead.
    /// </summary>
    public string? Name { get; set; }

    [Required]
    public string Body { get; set; }
}
