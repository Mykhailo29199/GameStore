using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories;

public interface IGameRepository : IGenericRepository<GameEntity>
{
    Task<bool> CheckIfKeyUniqueAsync(string gameKey);

    Task<GameEntity?> GetGameByKeyAsync(string gameKey);
}
