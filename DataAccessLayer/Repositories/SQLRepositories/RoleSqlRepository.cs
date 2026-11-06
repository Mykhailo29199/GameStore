using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class RoleSqlRepository(GameStoreContext context) : GenericSqlRepository<RoleEntity, Guid>(context), IRoleSqlRepository
{
    public async Task<RoleEntity?> GetWithPermissionsAsync(Guid id)
    {
        return await context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<string>> GetPermissionsAsync(IEnumerable<Guid> roleIds)
    {
        return await context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();
    }
}
