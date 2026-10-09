using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class PublisherRepository(GameStoreContext context) : GenericRepository<PublisherEntity>(context), IPublisherRepository
{
    private readonly DbSet<PublisherEntity> _dbSet = context.Set<PublisherEntity>();

    public async Task<IList<GameEntity>> GetGamesByPublisherNameAsync(string publisherName)
    {
        return await _dbSet
        .Where(p => p.CompanyName == publisherName)
        .SelectMany(p => p.Games)
        .ToListAsync();
    }

    public async Task<GameEntity> GetGamesByPublisherIdAsync(Guid id)
    {
        return await _dbSet
        .Where(p => p.Id == id)
        .SelectMany(p => p.Games)
        .FirstOrDefaultAsync();
    }
}