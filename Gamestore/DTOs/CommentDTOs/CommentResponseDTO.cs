namespace Gamestore.WebAPI.DTOs.CommentDTOs;

public class CommentResponseDTO
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Body { get; set; }

    public List<CommentResponseDTO> ChildComments { get; set; } = new();
}
