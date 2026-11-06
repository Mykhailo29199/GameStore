using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class UserSqlRepository(GameStoreContext context) : GenericSqlRepository<UserEntity, Guid>(context), IUserSqlRepository
{
    public async Task<UserEntity?> GetWithRolesAsync(Guid id)
    {
        return await WithRoles().FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserEntity?> GetByNameWithRolesAsync(string name)
    {
        return await WithRoles().FirstOrDefaultAsync(u => u.Name == name);
    }

    private IQueryable<UserEntity> WithRoles()
    {
        return context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role);
    }
}
