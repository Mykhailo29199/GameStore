using DataAccessLayer.Entities.NorthwindMDBEntities;
using MongoDB.Bson;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

public interface IProductMongoDBRepository : IMongoDBGenericRepository<ProductEntity>
{
    Task<ProductEntity?> GetByKeyAsync(string key);

    Task SetGameKeyAsync(ObjectId id, string key);

    Task IncrementViewsAsync(ObjectId id);

    Task SetUnitsInStockAsync(ObjectId id, int unitsInStock);
}
