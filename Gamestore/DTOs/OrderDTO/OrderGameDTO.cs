namespace Gamestore.WebAPI.DTOs.OrderDTO;

public class OrderGameDTO
{
    public Guid ProductId { get; set; }

    public double Price { get; set; }

    public int Quantity { get; set; }

    public int? Discount { get; set; }
}
