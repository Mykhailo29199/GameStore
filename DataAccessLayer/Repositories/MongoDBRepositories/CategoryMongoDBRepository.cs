using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

public class CategoryMongoDBRepository(IMongoDatabase database, IEntityChangeLogger changeLogger)
    : MongoDBGenericRepository<CategoryEntity>(database, "categories", changeLogger), ICategoryMongoDBRepository
{
    public async Task<List<CategoryEntity>> GetByIdsOrNamesAsync(IEnumerable<ObjectId> ids, IReadOnlyCollection<string> names)
    {
        return await Collection.Find(IdOrNameFilter(c => c.Name, ids, names)).ToListAsync();
    }

    public async Task<bool> ExistsByIdOrNameAsync(IEnumerable<ObjectId> ids, string name)
    {
        return await Collection.Find(IdOrNameFilter(c => c.Name, ids, [name])).AnyAsync();
    }
}
