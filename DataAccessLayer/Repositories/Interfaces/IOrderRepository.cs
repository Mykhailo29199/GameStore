using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IOrderRepository : IGenericRepository<OrderEntity>
{
    Task<OrderEntity?> GetOrderWithGamesAsync(Guid orderId);

    Task<OrderEntity?> GetActiveCartAsync(Guid customerId);
}
