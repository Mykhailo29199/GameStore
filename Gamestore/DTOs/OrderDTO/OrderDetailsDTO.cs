namespace Gamestore.WebAPI.DTOs.OrderDTO;

public class OrderDetailsDTO
{
    public Guid ProductId { get; set; }

    public double Price { get; set; } // total price

    public int Quantity { get; set; }

    public int? Discount { get; set; }
}