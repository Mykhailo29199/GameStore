using GameStore.Shared.Enums;

namespace BusinessLogicLayer.Models.OrderModels;

public class OrderModel
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public DateTime? Date { get; set; }

    public OrderStatus Status { get; set; }

    public List<OrderGameModel> Items { get; set; } = new();
}
