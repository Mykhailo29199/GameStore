using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IGameFileRepository : IGenericRepository<GameFileEntity>
{
    Task<GameFileEntity?> GetGameFileByKeyAsync(string gameKey);
}