using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Repositories.Interfaces.SQLInterfaces;
using DataAccessLayer.StoreContext;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.SQLRepositories;

public class OrderDetailsSqlRepository : GenericSqlRepository<OrderDetailsEntity, Guid>, IOrderDetailsSqlRepository
{
    private readonly GameStoreContext _context;

    public OrderDetailsSqlRepository(GameStoreContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<OrderDetailsEntity?> GetOrderGameAsync(Guid orderId, Guid productId)
    {
        return await _context.OrderDetails
            .FirstOrDefaultAsync(og => og.OrderId == orderId && og.ProductId == productId);
    }

    public async Task<List<OrderDetailsEntity>> GetOrderGamesAsync(Guid orderId)
    {
        return await _context.OrderDetails
            .Where(og => og.OrderId == orderId)
            .ToListAsync();
    }
}