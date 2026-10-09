using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IGameGenreRepository : IGenericRepository<GameGenreEntity>
{
    Task DeleteByGameIdAsync(Guid actualGameId);

    Task<IList<GameEntity>> GetGamesByGenreIdAsync(Guid genreId);

    Task<List<GameGenreEntity>> GetByGameIdAsync(Guid gameId);

    Task DeleteRangeAsync(IEnumerable<GameGenreEntity> entities);
}
