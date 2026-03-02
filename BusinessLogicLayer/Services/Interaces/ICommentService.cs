using BusinessLogicLayer.Models.CommentModels;

namespace BusinessLogicLayer.Services.Interaces;

public interface ICommentService
{
    // US1 – Add Comment
    Task AddCommentAsync(string gameKey, AddCommentModel model);

    // US2 – Get Comments
    Task<List<CommentModel>> GetGameCommentsAsync(string gameKey);

    // US3 – Delete (Soft delete)
    Task DeleteCommentAsync(string gameKey, Guid id);

    // US4 – Ban durations
    List<BanDurationOption> GetBanDurations();

    // US5 – Ban user
    Task BanUserAsync(BanModel model);
}
