using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IOrderDetailsSqlRepository : ISqlServerGenericRepository<OrderDetailsEntity>
{
    Task<OrderDetailsEntity?> GetOrderGameAsync(Guid orderId, Guid productId);

    Task<List<OrderDetailsEntity>> GetOrderGamesAsync(Guid orderId);
}
