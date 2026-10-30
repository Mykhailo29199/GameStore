using DataAccessLayer.Entities.NorthwindMDBEntities;

namespace DataAccessLayer.Repositories.Interfaces.MongoDBInterfaces;

public interface IEntityChangeLogRepository
{
    Task AddRangeAsync(IReadOnlyCollection<EntityChangeLogEntity> logs);

    void AddRange(IReadOnlyCollection<EntityChangeLogEntity> logs);
}
