using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class ProductCategorySqlRepository(GameStoreContext context) : GenericSqlRepository<ProductCategoryEntity, Guid>(context), IProductCategorySqlRepository
{
    private readonly DbSet<ProductCategoryEntity> _dbSet = context.Set<ProductCategoryEntity>();

    public async Task DeleteByGameIdAsync(Guid actualGameId)
    {
        var list = await _dbSet.Where(x => x.ProductId == actualGameId).ToListAsync();
        _dbSet.RemoveRange(list);
    }

    public async Task<IList<ProductEntity>> GetGamesByGenreIdAsync(Guid genreId)
    {
        return await _dbSet
            .Where(gg => gg.CategoryId == genreId)
            .Select(gg => gg.Product)
            .ToListAsync();
    }

    public async Task<List<ProductCategoryEntity>> GetByGameIdAsync(Guid gameId)
    {
        return await _dbSet.Where(x => x.ProductId == gameId).ToListAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<ProductCategoryEntity> entities)
    {
        _dbSet.RemoveRange(entities);
        await Task.CompletedTask;
    }
}
