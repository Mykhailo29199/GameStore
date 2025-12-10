using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IGameRepository : IGenericRepository<GameEntity>
{
    Task<GameEntity?> GetGameByKeyAsync(string gameKey);

    Task<IList<GameEntity>> GetGamesByPlatformIdAsync(Guid platformId);

    Task<IList<GameEntity>> GetGamesByGenresIdAsync(Guid platformId);

    Task<List<GenreEntity>> GetGenresByGameKeyAsync(string gameKey);
}
