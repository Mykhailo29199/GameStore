using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

public class OrderMongoDBRepository(IMongoDatabase database, IEntityChangeLogger changeLogger)
    : MongoDBGenericRepository<OrderMongoDBEntity>(database, "orders", changeLogger), IOrderMongoDBRepository
{
    public async Task<List<OrderMongoDBEntity>> GetByDateRangeAsync(DateTime? from, DateTime? to)
    {
        var query = Collection.AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(o => o.OrderDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(o => o.OrderDate < to.Value);
        }

        return await query.ToListAsync();
    }
}
