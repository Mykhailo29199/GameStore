using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IRegionRepository : ISqlServerGenericRepository<RegionEntity, Guid>
{
}
