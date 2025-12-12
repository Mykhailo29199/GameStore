using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IGamePlatformRepository : IGenericRepository<GamePlatformEntity>
{
    Task DeleteByGameIdAsync(Guid actualGameId);

    Task<IList<GameEntity>> GetGamesByPlatformIdAsync(Guid platformId);
}
