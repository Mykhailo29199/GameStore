using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IBanSqlRepository : ISqlServerGenericRepository<BanEntity, Guid>
{
}
