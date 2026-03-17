using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories.Interfaces;

public interface IOrderGameRepository : IGenericRepository<OrderGameEntity>
{
    Task<OrderGameEntity?> GetOrderGameAsync(Guid orderId, Guid productId);

    Task<List<OrderGameEntity>> GetOrderGamesAsync(Guid orderId);
}
