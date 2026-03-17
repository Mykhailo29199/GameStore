namespace Gamestore.WebAPI.DTOs.PaymentDTO.PaymentResponseDTOs;

public class PaymentResponseDTO
{
    public string Status { get; set; } // Success / Failed

    public string Method { get; set; }

    public string Message { get; set; }

    public BankResponseDTO BankResponseDTO { get; set; }

    public IBoxResponseDTO IBoxResponseDTO { get; set; }
}
