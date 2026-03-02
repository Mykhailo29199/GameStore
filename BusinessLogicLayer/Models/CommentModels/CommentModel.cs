namespace BusinessLogicLayer.Models.CommentModels;

public class CommentModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Body { get; set; }

    public Guid? ParentCommentId { get; set; }

    public Guid GameId { get; set; }

    public List<CommentModel> ChildComments { get; set; } = new();
}
