using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

public class SupplierMongoDBRepository(IMongoDatabase database, IEntityChangeLogger changeLogger)
    : MongoDBGenericRepository<SupplierEntity>(database, "suppliers", changeLogger), ISupplierMongoDBRepository
{
}
