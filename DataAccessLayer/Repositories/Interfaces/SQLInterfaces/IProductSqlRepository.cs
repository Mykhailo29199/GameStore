using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IProductSqlRepository : ISqlServerGenericRepository<ProductEntity>
{
    Task<ProductEntity> GetFullGame(Guid id);

    Task<ProductEntity?> GetGameByKeyAsync(string gameKey);

    Task<List<CategoryEntity>> GetGenresByGameKeyAsync(string gameKey);

    Task<List<PlatformEntity>> GetPlatformsByGameKeyAsync(string gameKey);

    Task<SupplierEntity> GetSupplierByGameKeyAsync(string gameKey);

    IQueryable<ProductEntity> GetQueryable();
}
