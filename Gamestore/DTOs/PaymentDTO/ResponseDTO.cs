namespace Gamestore.WebAPI.DTOs.PaymentDTO;

public class ResponseDTO
{
    public string Status { get; set; } // Success / Failed

    public string Method { get; set; }

    public Guid OrderId { get; set; }

    // Використовується для Bank/Visa
    public Guid? UserId { get; set; }

    public decimal Sum { get; set; }

    public DateTime PaymentDate { get; set; }

    // Використовується тільки для IBox
    public string? AccountNumber { get; set; }

    public string? InvoiceNumber { get; set; }

    // Використовується тільки для Bank
    public string? InvoiceUrl { get; set; }

    public byte[]? FileBytes { get; set; }
}
