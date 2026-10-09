using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class ProductSqlRepository(GameStoreContext context) : GenericSqlRepository<ProductEntity>(context), IProductSqlRepository
{
    private readonly DbSet<ProductEntity> _dbSet = context.Set<ProductEntity>();

    public async Task<ProductEntity> GetFullGame(Guid id)
    {
        return await GetDefaultIncludes().FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<ProductEntity?> GetGameByKeyAsync(string gameKey)
    {
        return await _dbSet.FirstOrDefaultAsync(game => game.Key == gameKey);
    }

    public async Task<List<CategoryEntity>> GetGenresByGameKeyAsync(string gameKey)
    {
        return await _dbSet
            .Where(g => g.Key == gameKey)
            .SelectMany(g => g.ProductCategories!.Select(gg => gg.Category))
            .ToListAsync();
    }

    public async Task<List<PlatformEntity>> GetPlatformsByGameKeyAsync(string gameKey)
    {
        return await _dbSet
            .Where(g => g.Key == gameKey)
            .SelectMany(g => g.ProductPlatforms!.Select(gp => gp.Platform))
            .ToListAsync();
    }

    public async Task<SupplierEntity> GetSupplierByGameKeyAsync(string gameKey)
    {
        var publisher = await _dbSet
        .Where(g => g.Key == gameKey)
        .Select(g => g.Supplier)
        .FirstOrDefaultAsync();

        return publisher;
    }

    public IQueryable<ProductEntity> GetQueryable()
    {
        return GetDefaultIncludes();
    }

    private IQueryable<ProductEntity> GetDefaultIncludes()
    {
        return _dbSet
            .Include(g => g.Supplier)
            .Include(g => g.ProductCategories)
                .ThenInclude(gg => gg.Category)
            .Include(g => g.ProductPlatforms)
                .ThenInclude(gp => gp.Platform)
            .Include(g => g.Comments);
    }
}
