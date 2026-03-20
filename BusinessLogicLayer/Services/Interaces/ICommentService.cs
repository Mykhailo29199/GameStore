using BusinessLogicLayer.Models.CommentModels;

namespace BusinessLogicLayer.Services.Interaces;

public interface ICommentService
{
    Task AddCommentAsync(string gameKey, AddCommentModel model);

    Task<List<CommentModel>> GetGameCommentsAsync(string gameKey);

    Task DeleteCommentAsync(string gameKey, Guid id);
}
