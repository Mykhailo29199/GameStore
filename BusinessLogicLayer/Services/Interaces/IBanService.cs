using BusinessLogicLayer.Models.CommentModels;

namespace BusinessLogicLayer.Services.Interaces;

public interface IBanService
{
    List<BanDurationOption> GetBanDurations();

    Task BanUserAsync(BanModel model);
}
