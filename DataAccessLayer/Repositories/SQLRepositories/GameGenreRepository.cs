using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class GameGenreRepository(GameStoreContext context) : GenericRepository<GameGenreEntity>(context), IGameGenreRepository
{
    private readonly DbSet<GameGenreEntity> _dbSet = context.Set<GameGenreEntity>();

    public async Task DeleteByGameIdAsync(Guid actualGameId)
    {
        var list = await _dbSet.Where(x => x.GameId == actualGameId).ToListAsync();
        _dbSet.RemoveRange(list);
    }

    public async Task<IList<GameEntity>> GetGamesByGenreIdAsync(Guid genreId)
    {
        return await _dbSet
            .Where(gg => gg.GenreId == genreId)
            .Select(gg => gg.Game!)
            .ToListAsync();
    }

    public async Task<List<GameGenreEntity>> GetByGameIdAsync(Guid gameId)
    {
        return await _dbSet.Where(x => x.GameId == gameId).ToListAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<GameGenreEntity> entities)
    {
        _dbSet.RemoveRange(entities);
        await Task.CompletedTask;
    }
}
