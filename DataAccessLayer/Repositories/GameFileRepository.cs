using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public class GameFileRepository(GameStoreContext context) : GenericRepository<GameFileEntity>(context), IGameFileRepository
{
    private readonly DbSet<GameFileEntity> _dbSet = context.Set<GameFileEntity>();

    public async Task<GameFileEntity?> GetGameFileByKeyAsync(string gameKey)
    {
        var file = await _dbSet.FirstOrDefaultAsync(x => x.Game != null && x.Game.Key == gameKey);
        return file;
    }
}
