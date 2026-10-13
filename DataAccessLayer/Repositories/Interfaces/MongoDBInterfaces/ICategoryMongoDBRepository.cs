using DataAccessLayer.Entities.NorthwindMDBEntities;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

public interface ICategoryMongoDBRepository : IMongoDBGenericRepository<CategoryEntity>
{
}
