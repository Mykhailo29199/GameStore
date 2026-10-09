using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IGameRepository : IGenericRepository<GameEntity>
{
    Task<GameEntity> GetFullGame(Guid id);

    Task<GameEntity?> GetGameByKeyAsync(string gameKey);

    Task<List<GenreEntity>> GetGenresByGameKeyAsync(string gameKey);

    Task<List<PlatformEntity>> GetPlatformsByGameKeyAsync(string gameKey);

    Task<PublisherEntity> GetPublisherByGameKeyAsync(string gameKey);

    IQueryable<GameEntity> GetQueryable();
}
