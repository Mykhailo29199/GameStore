using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class OrderGameRepository : GenericRepository<OrderGameEntity>, IOrderGameRepository
{
    private readonly GameStoreContext _context;

    public OrderGameRepository(GameStoreContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<OrderGameEntity?> GetOrderGameAsync(Guid orderId, Guid productId)
    {
        return await _context.OrderGames
            .FirstOrDefaultAsync(og => og.OrderId == orderId && og.ProductId == productId);
    }

    public async Task<List<OrderGameEntity>> GetOrderGamesAsync(Guid orderId)
    {
        return await _context.OrderGames
            .Where(og => og.OrderId == orderId)
            .ToListAsync();
    }
}