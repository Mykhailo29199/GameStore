using DataAccessLayer.Entities.NorthwindMDBEntities;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

public interface IOrderMongoDBRepository : IMongoDBGenericRepository<OrderMongoDBEntity>
{
    Task<List<OrderMongoDBEntity>> GetByDateRangeAsync(DateTime? from, DateTime? to);
}
