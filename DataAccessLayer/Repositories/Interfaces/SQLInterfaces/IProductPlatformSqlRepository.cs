using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IProductPlatformSqlRepository : ISqlServerGenericRepository<ProductPlatformEntity, Guid>
{
    Task DeleteByGameIdAsync(Guid actualGameId);

    Task<IList<ProductEntity>> GetGamesByPlatformIdAsync(Guid platformId);

    Task<List<ProductPlatformEntity>> GetByGameIdAsync(Guid gameId);

    Task DeleteRangeAsync(IEnumerable<ProductPlatformEntity> entities);
}
