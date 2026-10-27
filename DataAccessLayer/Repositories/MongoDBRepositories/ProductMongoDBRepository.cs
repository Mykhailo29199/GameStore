using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

public class ProductMongoDBRepository(IMongoDatabase database, IEntityChangeLogger changeLogger)
    : MongoDBGenericRepository<ProductEntity>(database, "products", changeLogger), IProductMongoDBRepository
{
    public async Task<ProductEntity?> GetByKeyAsync(string key)
    {
        return await Collection.AsQueryable()
            .Where(p => p.Key == key)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ProductEntity>> GetByIdsOrKeysAsync(IEnumerable<ObjectId> ids, IReadOnlyCollection<string> keys)
    {
        return await Collection.Find(IdOrNameFilter(p => p.Key, ids, keys)).ToListAsync();
    }

    public async Task SetGameKeyAsync(ObjectId id, string key)
    {
        var filter = Builders<ProductEntity>.Filter.Eq(p => p.InternalId, id);
        var update = Builders<ProductEntity>.Update.Set(p => p.Key, key);

        await UpdateAsync(filter, update);
    }

    public async Task IncrementViewsAsync(ObjectId id)
    {
        var filter = Builders<ProductEntity>.Filter.Eq(p => p.InternalId, id);
        var update = Builders<ProductEntity>.Update.Inc(p => p.Views, 1);

        await UpdateAsync(filter, update);
    }

    public async Task SetUnitsInStockAsync(ObjectId id, int unitsInStock)
    {
        var filter = Builders<ProductEntity>.Filter.Eq(p => p.InternalId, id);
        var update = Builders<ProductEntity>.Update.Set(p => p.UnitsInStock, unitsInStock);

        await UpdateAsync(filter, update);
    }
}
