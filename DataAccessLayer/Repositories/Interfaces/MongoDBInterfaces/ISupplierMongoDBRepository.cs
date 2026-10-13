using DataAccessLayer.Entities.NorthwindMDBEntities;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

public interface ISupplierMongoDBRepository : IMongoDBGenericRepository<SupplierEntity>
{
}
