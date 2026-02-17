using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public class OrderGame
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid GameId { get; set; }

    [Required]
    public double Price { get; set; }

    [Required]
    public int Quantity { get; set; }

    public int? Discount { get; set; }

    public Order Order { get; set; }

    public GameEntity Game { get; set; }
}
