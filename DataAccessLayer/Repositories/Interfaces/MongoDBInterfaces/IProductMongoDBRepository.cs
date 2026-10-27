using DataAccessLayer.Entities.NorthwindMDBEntities;
using MongoDB.Bson;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

public interface IProductMongoDBRepository : IMongoDBGenericRepository<ProductEntity>
{
    Task<ProductEntity?> GetByKeyAsync(string key);

    /// <summary>
    /// Returns documents with one of the ids or one of the game keys (ignoring case and surrounding spaces).
    /// </summary>
    Task<List<ProductEntity>> GetByIdsOrKeysAsync(IEnumerable<ObjectId> ids, IReadOnlyCollection<string> keys);

    Task SetGameKeyAsync(ObjectId id, string key);

    Task IncrementViewsAsync(ObjectId id);

    Task SetUnitsInStockAsync(ObjectId id, int unitsInStock);
}
