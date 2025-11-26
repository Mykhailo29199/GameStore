using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IGameRepository : IGenericRepository<GameEntity>
{
    Task<GameEntity?> GetGameByKeyAsync(string gameKey);
}
