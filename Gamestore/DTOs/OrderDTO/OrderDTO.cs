namespace Gamestore.WebAPI.DTOs.OrderDTO;

public class OrderDTO
{
    public Guid Id { get; set; }

    // Northwind customers have text identifiers (for example "VINET"), so the value is not always a Guid.
    public string CustomerId { get; set; }

    // Damaged Northwind dates are returned as null.
    public DateTime? Date { get; set; }
}
