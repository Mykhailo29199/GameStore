using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IProductCategorySqlRepository : ISqlServerGenericRepository<ProductCategoryEntity>
{
    Task DeleteByGameIdAsync(Guid actualGameId);

    Task<IList<ProductEntity>> GetGamesByGenreIdAsync(Guid genreId);

    Task<List<ProductCategoryEntity>> GetByGameIdAsync(Guid gameId);

    Task DeleteRangeAsync(IEnumerable<ProductCategoryEntity> entities);
}
