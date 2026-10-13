using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IProductFileSqlRepository : ISqlServerGenericRepository<ProductFileEntity, Guid>
{
    Task<ProductFileEntity?> GetGameFileByKeyAsync(string gameKey);
}