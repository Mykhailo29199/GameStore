using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface ICategorySqlRepository : ISqlServerGenericRepository<CategoryEntity>
{
    Task<CategoryEntity?> GetWithSubGenresAsync(Guid id);
}
