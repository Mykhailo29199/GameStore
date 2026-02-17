namespace BusinessLogicLayer.Models.Payment;

public class BankPaymentModel
{
    public Guid UserId { get; set; }

    public decimal Sum { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ValidUntil { get; set; }
}