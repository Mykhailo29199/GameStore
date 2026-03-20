using BusinessLogicLayer.Models.BanModels;

namespace BusinessLogicLayer.Services.Interaces;

public interface IBanService
{
    List<string> GetBanDurations();

    Task BanUserAsync(BanModel model);
}
