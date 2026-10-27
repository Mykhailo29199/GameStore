using DataAccessLayer.Entities.NorthwindMDBEntities;
using MongoDB.Bson;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

public interface ISupplierMongoDBRepository : IMongoDBGenericRepository<SupplierEntity>
{
    /// <summary>
    /// Returns documents with one of the ids or one of the names (ignoring case and surrounding spaces).
    /// </summary>
    Task<List<SupplierEntity>> GetByIdsOrNamesAsync(IEnumerable<ObjectId> ids, IReadOnlyCollection<string> names);

    Task<bool> ExistsByIdOrNameAsync(IEnumerable<ObjectId> ids, string name);
}
