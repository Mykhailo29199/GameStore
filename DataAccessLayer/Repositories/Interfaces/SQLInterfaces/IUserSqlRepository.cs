using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IUserSqlRepository : ISqlServerGenericRepository<UserEntity, Guid>
{
    Task<UserEntity?> GetWithRolesAsync(Guid id);

    Task<UserEntity?> GetByNameWithRolesAsync(string name);
}
