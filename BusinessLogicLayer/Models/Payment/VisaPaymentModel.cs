namespace BusinessLogicLayer.Models.Payment;

public class VisaPaymentModel
{
    public decimal TotalAmount { get; set; }

    public string CardHolderName { get; set; }

    public string CardNumber { get; set; }

    public int ExpirationMonth { get; set; }

    public int ExpirationYear { get; set; }

    public int Cvv { get; set; }
}
