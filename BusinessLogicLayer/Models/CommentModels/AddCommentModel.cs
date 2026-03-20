using GameStore.Shared.Enums;

namespace BusinessLogicLayer.Models.CommentModels;

public class AddCommentModel
{
    public string Name { get; set; }

    public string Body { get; set; }

    public Guid? ParentId { get; set; }

    public CommentActions? Action { get; set; }
}
