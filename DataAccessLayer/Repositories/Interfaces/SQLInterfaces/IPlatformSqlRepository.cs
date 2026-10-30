using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IPlatformSqlRepository : ISqlServerGenericRepository<PlatformEntity, Guid>
{
}
