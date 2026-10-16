using BusinessLogicLayer.Models.OrderModels;

namespace BusinessLogicLayer.Services.Interaces;

public interface IOrderService
{
    Task AddGameToCartAsync(Guid userId, string gameKey);

    Task RemoveGameFromCartAsync(Guid userId, string gameKey);

    Task<IEnumerable<OrderModel>> GetClosedOrdersAsync();

    Task<IEnumerable<OrderGameModel>> GetOrderDetailsAsync(Guid orderId);

    Task<OrderModel?> GetOrderByIdAsync(Guid orderId);

    Task<IEnumerable<OrderGameModel>> GetCartAsync(Guid userId);

    Task<IEnumerable<OrderModel>> GetOrdersHistoryAsync(DateTime? startDate = null, DateTime? endDate = null);

    Task UpdateOrderDetailQuantityAsync(Guid orderDetailId, int count);

    Task DeleteOrderDetailAsync(Guid orderDetailId);

    Task ShipOrderAsync(Guid orderId);

    Task AddGameToOrderAsync(Guid orderId, string gameKey);
}
