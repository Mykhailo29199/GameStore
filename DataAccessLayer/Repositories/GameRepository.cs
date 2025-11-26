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
}
