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

    public async Task<IList<GameEntity>> GetGamesByPlatformIdAsync(Guid platformId)
    {
        return await _dbSet.Where(game => game.GamePlatforms != null && game.GamePlatforms.Any(gp => gp.PlatformId == platformId))
                           .ToListAsync();
    }

    public async Task<IList<GameEntity>> GetGamesByGenresIdAsync(Guid platformId)
    {
        return await _dbSet.Where(game => game.GameGenres != null && game.GameGenres.Any(gg => gg.GenreId == platformId))
                           .ToListAsync();
    }

    public async Task<List<GenreEntity>> GetGenresByGameKeyAsync(string gameKey)
    {
        return await _dbSet
            .Where(g => g.GameKey == gameKey)
            .SelectMany(g => g.GameGenres!.Select(gg => gg.Genre))
            .ToListAsync();
    }
}
