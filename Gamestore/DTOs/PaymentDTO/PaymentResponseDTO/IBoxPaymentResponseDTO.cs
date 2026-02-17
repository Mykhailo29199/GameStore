namespace Gamestore.WebAPI.DTOs.PaymentDTO.PaymentResponseDTO;

public class IBoxPaymentResponseDTO : BaseResponseDTO
{
    public Guid UserId { get; set; }

    public Guid OrderId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Sum { get; set; }
}
