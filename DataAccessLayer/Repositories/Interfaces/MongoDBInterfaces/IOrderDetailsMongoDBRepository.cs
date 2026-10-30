using DataAccessLayer.Entities.NorthwindMDBEntities;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

public interface IOrderDetailsMongoDBRepository : IMongoDBGenericRepository<OrderDetailsEntity>
{
    Task<List<OrderDetailsEntity>> GetByOrderIdAsync(int orderId);
}
