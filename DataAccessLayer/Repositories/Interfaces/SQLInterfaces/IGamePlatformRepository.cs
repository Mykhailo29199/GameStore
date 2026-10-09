using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IGamePlatformRepository : IGenericRepository<GamePlatformEntity>
{
    Task DeleteByGameIdAsync(Guid actualGameId);

    Task<IList<GameEntity>> GetGamesByPlatformIdAsync(Guid platformId);

    Task<List<GamePlatformEntity>> GetByGameIdAsync(Guid gameId);

    Task DeleteRangeAsync(IEnumerable<GamePlatformEntity> entities);
}
