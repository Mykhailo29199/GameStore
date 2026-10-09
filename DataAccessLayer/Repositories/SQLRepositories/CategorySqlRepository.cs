using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class CategorySqlRepository(GameStoreContext context) : GenericSqlRepository<CategoryEntity>(context), ICategorySqlRepository
{
    private readonly DbSet<CategoryEntity> _dbSet = context.Set<CategoryEntity>();

    public async Task<CategoryEntity?> GetWithSubGenresAsync(Guid id)
    {
        return await _dbSet
            .Include(g => g.SubGenres) // Завантажуємо дочірні елементи в Change Tracker
            .FirstOrDefaultAsync(g => g.Id == id);
    }
}
