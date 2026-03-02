using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.CommentDTOs;

public class CommentContentDTO
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Body { get; set; }
}
