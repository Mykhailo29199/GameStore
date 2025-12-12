using BusinessLogicLayer.Models;

namespace BusinessLogicLayer.Services.Interaces;

public interface IPlatformService
{
    Task CreatePlatformAsync(string platformType);

    Task<PlatformModel> GetPlatformByIdAsync(Guid platformId);

    Task<List<PlatformModel>> GetAllPlatformsAsync();

    Task DeletePlatformAsync(Guid platformId);

    Task UpdatePlatformAsync(PlatformModel platformModel);

    Task<List<GameModel>> GetGamesByPlatformIdAsync(Guid platformId);
}
