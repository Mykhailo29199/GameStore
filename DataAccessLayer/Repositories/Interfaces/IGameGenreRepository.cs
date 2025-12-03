using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IGameGenreRepository : IGenericRepository<GameGenreEntity>
{
    Task DeleteByGameIdAsync(Guid actualGameId);
}
