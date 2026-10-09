using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IOrderRepository : IGenericRepository<OrderEntity>
{
    Task<OrderEntity?> GetOrderWithGamesAsync(Guid orderId);

    Task<OrderEntity?> GetActiveCartAsync(Guid customerId);
}
