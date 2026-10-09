using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IOrderSqlRepository : ISqlServerGenericRepository<OrderEntity>
{
    Task<OrderEntity?> GetOrderWithGamesAsync(Guid orderId);

    Task<OrderEntity?> GetActiveCartAsync(Guid customerId);
}
