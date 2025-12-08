using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IGameFileRepository : IGenericRepository<GameFileEntity>
{
    Task<GameFileEntity?> GetGameFileByKeyAsync(string gameKey);
}