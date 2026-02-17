using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.PaymentDTO;

public class PaymentRequestDTO
{
    [Required]
    [RegularExpression(@"^(Bank|IBox terminal|Visa)$", ErrorMessage = "Allowed payment methods: Bank, IBox terminal, Visa.")]
    public string Method { get; set; }

    public Guid OrderId { get; set; }

    public VisaPaymentDTO? VisaModel { get; set; }

    public IBoxPaymentDTO? IBoxModel { get; set; }

    public BankPaymentDTO? BankModel { get; set; }
}