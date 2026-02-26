namespace BusinessLogicLayer.Models.OrderModels;

public class OrderGameModel
{
    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public double Price { get; set; }

    public int Quantity { get; set; }

    public int? Discount { get; set; }

    // Додатково для сервісного шару
    public string GameName { get; set; }
}
