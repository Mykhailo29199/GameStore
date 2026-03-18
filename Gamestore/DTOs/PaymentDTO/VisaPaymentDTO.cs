using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.PaymentDTO;

public class VisaPaymentDTO
{
    //[Required]
    //[Range(0.01, 1_000_000, ErrorMessage = "Transaction amount must be greater than 0.")]
    //public double TransactionAmount { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Card holder name must be between 2 and 100 characters.")]
    [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "Card holder name contains invalid characters.")]
    public string Holder { get; set; }

    [Required]
    [RegularExpression(@"^4[0-9]{15}$", ErrorMessage = "Card number must be a valid 16-digit Visa card.")]
    public string CardNumber { get; set; }

    [Required]
    [Range(1, 12, ErrorMessage = "Expiration month must be between 1 and 12.")]
    public int MonthExpire { get; set; }

    public int YearExpire { get; set; }

    [Required]
    [RegularExpression(@"^[0-9]{3}$", ErrorMessage = "CVV must be exactly 3 digits.")]
    public int Cvv2 { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        _ = validationContext;
        var now = DateTime.UtcNow;

        if (YearExpire < now.Year)
        {
            yield return new ValidationResult(
                "Card expiration year cannot be in the past.",
                [nameof(YearExpire)]);
        }

        if (YearExpire == now.Year &&
            MonthExpire < now.Month)
        {
            yield return new ValidationResult(
                "Card expiration date cannot be in the past.",
                [nameof(MonthExpire), nameof(YearExpire)]);
        }
    }
}
