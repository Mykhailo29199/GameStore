using DataAccessLayer.Entities.NorthwindMDBEntities;
using DataAccessLayer.Logging;
using DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;
using MongoDB.Driver;

namespace DataAccessLayer.Repositories.MongoDBRepositories;

public class CategoryMongoDBRepository(IMongoDatabase database, IEntityChangeLogger changeLogger)
    : MongoDBGenericRepository<CategoryEntity>(database, "categories", changeLogger), ICategoryMongoDBRepository
{
}
