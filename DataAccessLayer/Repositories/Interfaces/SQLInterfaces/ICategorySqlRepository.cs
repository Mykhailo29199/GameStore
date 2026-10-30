using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface ICategorySqlRepository : ISqlServerGenericRepository<CategoryEntity, Guid>
{
    Task<CategoryEntity?> GetWithSubGenresAsync(Guid id);
}
