using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.PaymentDTO;

public class IBoxPaymentDTO
{
    [Required]
    [Range(0.01, 1_000_000, ErrorMessage = "Transaction amount must be greater than 0.")]
    public double TransactionAmount { get; set; }

    [Required]
    public Guid AccountNumber { get; set; }

    [Required]
    public Guid InvoiceNumber { get; set; }
}