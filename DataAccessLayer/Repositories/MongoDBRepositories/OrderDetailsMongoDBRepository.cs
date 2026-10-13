using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

public class OrderDetailsMongoDBRepository(IMongoDatabase database, IEntityChangeLogger changeLogger)
    : MongoDBGenericRepository<OrderDetailsEntity>(database, "order-details", changeLogger), IOrderDetailsMongoDBRepository
{
    public async Task<List<OrderDetailsEntity>> GetByOrderIdAsync(int orderId)
    {
        return await Collection.AsQueryable()
            .Where(d => d.OrderID == orderId)
            .ToListAsync();
    }
}
