using GameStore.Shared.Enums;

namespace BusinessLogicLayer.Models.OrderModels;

public class OrderModel
{
    public string Id { get; set; }

    public string CustomerId { get; set; }

    public DateTime? Date { get; set; }

    public OrderStatus Status { get; set; }
}
