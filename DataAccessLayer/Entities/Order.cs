using System.ComponentModel.DataAnnotations;
using GameStore.Shared.Enums;

namespace DataAccessLayer.Entities;

public class Order
{
    [Required]
    public Guid Id { get; set; }

    public DateTime? Date { get; set; }

    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public OrderStatus Status { get; set; }

    public ICollection<OrderGame> OrderGames { get; set; } = new List<OrderGame>();
}
