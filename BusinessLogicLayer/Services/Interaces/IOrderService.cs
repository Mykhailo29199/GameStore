using BusinessLogicLayer.Models.OrderModels;

namespace BusinessLogicLayer.Services.Interaces;

public interface IOrderService
{
    Task AddGameAsync(Guid userId, string gameKey);

    Task RemoveGameAsync(Guid? userId, string gameKey);

    Task<IEnumerable<OrderModel>> GetClosedOrdersAsync();

    Task<IEnumerable<OrderGameModel>> GetOrderDetailsAsync(Guid orderId);

    Task<OrderModel?> GetOrderByIdAsync(Guid orderId);

    Task<IEnumerable<OrderGameModel>> GetCartAsync(Guid userId);
}
