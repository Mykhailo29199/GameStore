using GameStore.Shared.Enums;

namespace BusinessLogicLayer.Models.OrderModels;

public class OrderModel
{
    public Guid Id { get; set; }

    public string CustomerId { get; set; }

    public DateTime? Date { get; set; }

    public OrderStatus Status { get; set; }

    public decimal? Sum { get; set; }
}
