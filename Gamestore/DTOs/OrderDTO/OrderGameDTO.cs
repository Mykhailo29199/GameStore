namespace Gamestore.WebAPI.DTOs.OrderDTO;

public class OrderGameDTO
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public int? Discount { get; set; }
}
