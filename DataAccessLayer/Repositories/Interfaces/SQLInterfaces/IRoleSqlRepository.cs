using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IRoleSqlRepository : ISqlServerGenericRepository<RoleEntity, Guid>
{
    Task<RoleEntity?> GetWithPermissionsAsync(Guid id);

    Task<List<string>> GetPermissionsAsync(IEnumerable<Guid> roleIds);
}
