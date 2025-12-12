using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IGameRepository : IGenericRepository<GameEntity>
{
    Task<GameEntity?> GetGameByKeyAsync(string gameKey);

    Task<List<GenreEntity>> GetGenresByGameKeyAsync(string gameKey);

    Task<List<PlatformEntity>> GetPlatformsByGameKeyAsync(string gameKey);
}
