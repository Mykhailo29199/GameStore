using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class GameRepository(GameStoreContext context) : GenericRepository<GameEntity>(context), IGameRepository
{
    private readonly DbSet<GameEntity> _dbSet = context.Set<GameEntity>();

    public async Task<GameEntity?> GetGameByKeyAsync(string gameKey)
    {
        return await _dbSet.FirstOrDefaultAsync(game => game.GameKey == gameKey);
    }

    public async Task<List<GenreEntity>> GetGenresByGameKeyAsync(string gameKey)
    {
        return await _dbSet
            .Where(g => g.GameKey == gameKey)
            .SelectMany(g => g.GameGenres!.Select(gg => gg.Genre))
            .ToListAsync();
    }

    public async Task<List<PlatformEntity>> GetPlatformsByGameKeyAsync(string gameKey)
    {
        return await _dbSet
            .Where(g => g.GameKey == gameKey)
            .SelectMany(g => g.GamePlatforms!.Select(gp => gp.Platform))
            .ToListAsync();
    }
}
