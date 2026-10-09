using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class ProductPlatformSqlRepository(GameStoreContext context) : GenericSqlRepository<ProductPlatformEntity>(context), IProductPlatformSqlRepository
{
    private readonly DbSet<ProductPlatformEntity> _dbSet = context.Set<ProductPlatformEntity>();

    public async Task DeleteByGameIdAsync(Guid actualGameId)
    {
        var list = await _dbSet.Where(x => x.ProductId == actualGameId).ToListAsync();
        _dbSet.RemoveRange(list);
    }

    public async Task<IList<ProductEntity>> GetGamesByPlatformIdAsync(Guid platformId)
    {
        return await _dbSet
            .Where(gp => gp.PlatformId == platformId)
            .Select(gp => gp.Product!)
            .ToListAsync();
    }

    public async Task<List<ProductPlatformEntity>> GetByGameIdAsync(Guid gameId)
    {
        return await _dbSet.Where(x => x.ProductId == gameId).ToListAsync();
    }

    public Task DeleteRangeAsync(IEnumerable<ProductPlatformEntity> entities)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }
}
