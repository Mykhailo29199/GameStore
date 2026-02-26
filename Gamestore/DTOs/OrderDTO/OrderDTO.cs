namespace Gamestore.WebAPI.DTOs.OrderDTO;

public class OrderDTO
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public DateTime Date { get; set; }
}
