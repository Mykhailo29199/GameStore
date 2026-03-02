namespace BusinessLogicLayer.Models.CommentModels;

public class AddCommentModel
{
    public string Name { get; set; }

    public string Body { get; set; }

    public Guid? ParentId { get; set; }

    public string? Action { get; set; } // "reply" | "quote"
}
