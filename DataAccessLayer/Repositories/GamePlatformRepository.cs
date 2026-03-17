using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class GamePlatformRepository(GameStoreContext context) : GenericRepository<GamePlatformEntity>(context), IGamePlatformRepository
{
    private readonly DbSet<GamePlatformEntity> _dbSet = context.Set<GamePlatformEntity>();

    public async Task DeleteByGameIdAsync(Guid actualGameId)
    {
        var list = await _dbSet.Where(x => x.GameId == actualGameId).ToListAsync();
        _dbSet.RemoveRange(list);
    }

    public async Task<IList<GameEntity>> GetGamesByPlatformIdAsync(Guid platformId)
    {
        return await _dbSet
            .Where(gp => gp.PlatformId == platformId)
            .Select(gp => gp.Game!)
            .ToListAsync();
    }

    public async Task<List<GamePlatformEntity>> GetByGameIdAsync(Guid gameId)
    {
        return await _dbSet.Where(x => x.GameId == gameId).ToListAsync();
    }

    public Task DeleteRangeAsync(IEnumerable<GamePlatformEntity> entities)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }
}
