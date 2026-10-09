using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using GameStore.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class OrderRepository : GenericRepository<OrderEntity>, IOrderRepository
{
    private readonly GameStoreContext _context;

    public OrderRepository(GameStoreContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<OrderEntity?> GetOrderWithGamesAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
            .ThenInclude(og => og.Game)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<OrderEntity?> GetActiveCartAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.OrderGames)
            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.Status == OrderStatus.Open);
    }
}
