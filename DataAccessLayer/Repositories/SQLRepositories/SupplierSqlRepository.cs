using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class SupplierSqlRepository(GameStoreContext context) : GenericSqlRepository<SupplierEntity>(context), ISupplierSqlRepository
{
    private readonly DbSet<SupplierEntity> _dbSet = context.Set<SupplierEntity>();

    public async Task<IList<ProductEntity>> GetProductsByPublisherNameAsync(string publisherName)
    {
        return await _dbSet
        .Where(p => p.CompanyName == publisherName)
        .SelectMany(p => p.Products)
        .ToListAsync();
    }

    public async Task<ProductEntity> GetProductsByPublisherIdAsync(Guid id)
    {
        return await _dbSet
        .Where(p => p.Id == id)
        .SelectMany(p => p.Products)
        .FirstOrDefaultAsync();
    }
}