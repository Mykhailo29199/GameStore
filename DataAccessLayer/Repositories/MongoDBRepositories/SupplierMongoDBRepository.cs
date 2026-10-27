using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

public class SupplierMongoDBRepository(IMongoDatabase database, IEntityChangeLogger changeLogger)
    : MongoDBGenericRepository<SupplierEntity>(database, "suppliers", changeLogger), ISupplierMongoDBRepository
{
    public async Task<List<SupplierEntity>> GetByIdsOrNamesAsync(IEnumerable<ObjectId> ids, IReadOnlyCollection<string> names)
    {
        return await Collection.Find(IdOrNameFilter(s => s.CompanyName, ids, names)).ToListAsync();
    }

    public async Task<bool> ExistsByIdOrNameAsync(IEnumerable<ObjectId> ids, string name)
    {
        return await Collection.Find(IdOrNameFilter(s => s.CompanyName, ids, [name])).AnyAsync();
    }
}
