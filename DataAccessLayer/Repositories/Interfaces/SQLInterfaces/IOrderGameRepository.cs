using DataAccessLayer.Entities.GameStoreSQLEntities;

namespace DataAccessLayer.Repositories.Interfaces.SQLInterfaces;

public interface IOrderGameRepository : IGenericRepository<OrderGameEntity>
{
    Task<OrderGameEntity?> GetOrderGameAsync(Guid orderId, Guid productId);

    Task<List<OrderGameEntity>> GetOrderGamesAsync(Guid orderId);
}
