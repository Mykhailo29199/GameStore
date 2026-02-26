namespace Gamestore.WebAPI.DTOs.PaymentDTO.PaymentResponseDTOs;

public class IBoxResponseDTO
{
    public Guid OrderId { get; set; }

    // Використовується для Bank/Visa
    public Guid? UserId { get; set; }

    public decimal Sum { get; set; }

    public DateTime PaymentDate { get; set; }
}