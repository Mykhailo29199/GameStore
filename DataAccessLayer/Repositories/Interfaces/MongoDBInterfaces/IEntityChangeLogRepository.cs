using DataAccessLayer.Entities.NorthwindMDBEntities;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

public interface IEntityChangeLogRepository
{
    Task AddAsync(EntityChangeLogEntity log);
}
