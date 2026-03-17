using DataAccessLayer.Entities;
using DataAccessLayer.Repositories.Interfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

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