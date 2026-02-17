namespace Gamestore.WebAPI.DTOs.PaymentDTO.PaymentResponseDTO;

public class BankPaymentResponseDTO : BaseResponseDTO
{
    public Guid UserId { get; set; }

    public Guid OrderId { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ValidUntil { get; set; }

    public decimal Sum { get; set; }
}
