using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class GameRepository(GameStoreContext context) : GenericRepository<GameEntity>(context), IGameRepository
{
    private readonly DbSet<GameEntity> _dbSet = context.Set<GameEntity>();

    public async Task<GameEntity> GetFullGame(Guid id)
    {
        return await _dbSet
            .Include(g => g.Publisher)
            .Include(g => g.GameGenres)
                .ThenInclude(gg => gg.Genre)
            .Include(g => g.GamePlatforms)
                .ThenInclude(gp => gp.Platform)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GameEntity?> GetGameByKeyAsync(string gameKey)
    {
        return await _dbSet.FirstOrDefaultAsync(game => game.Key == gameKey);
    }

    public async Task<List<GenreEntity>> GetGenresByGameKeyAsync(string gameKey)
    {
        return await _dbSet
            .Where(g => g.Key == gameKey)
            .SelectMany(g => g.GameGenres!.Select(gg => gg.Genre))
            .ToListAsync();
    }

    public async Task<List<PlatformEntity>> GetPlatformsByGameKeyAsync(string gameKey)
    {
        return await _dbSet
            .Where(g => g.Key == gameKey)
            .SelectMany(g => g.GamePlatforms!.Select(gp => gp.Platform))
            .ToListAsync();
    }

    public async Task<PublisherEntity> GetPublisherByGameKeyAsync(string gameKey)
    {
        var publisher = await _dbSet
        .Where(g => g.Key == gameKey)
        .Select(g => g.Publisher)
        .FirstOrDefaultAsync();

        return publisher;
    }

    public IQueryable<GameEntity> GetQueryable()
    {
        return _dbSet
            .Include(g => g.Publisher)
            .Include(g => g.GameGenres)
                .ThenInclude(gg => gg.Genre)
            .Include(g => g.GamePlatforms)
                .ThenInclude(gp => gp.Platform)
            .Include(g => g.Comments);
    }
}
