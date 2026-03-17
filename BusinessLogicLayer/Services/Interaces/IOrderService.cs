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
}
