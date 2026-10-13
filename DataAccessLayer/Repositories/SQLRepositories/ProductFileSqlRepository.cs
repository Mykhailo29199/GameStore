using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class ProductFileSqlRepository(GameStoreContext context) : GenericSqlRepository<ProductFileEntity, Guid>(context), IProductFileSqlRepository
{
    private readonly DbSet<ProductFileEntity> _dbSet = context.Set<ProductFileEntity>();

    public async Task<ProductFileEntity?> GetGameFileByKeyAsync(string gameKey)
    {
        var file = await _dbSet.FirstOrDefaultAsync(x => x.Product != null && x.Product.Key == gameKey);
        return file;
    }
}
