using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.PaymentDTO;

public class BankPaymentDTO
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [Range(0.01, 1_000_000, ErrorMessage = "Transaction amount must be greater than 0.")]
    public decimal Sum { get; set; }

    // Дата створення інвойсу
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    // Валідність інвойсу (наприклад, 7 днів)
    public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(7);
}
