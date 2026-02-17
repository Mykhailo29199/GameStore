namespace BusinessLogicLayer.Models.Payment;

public class PaymentResponseModel
{
    public string Status { get; set; } // Success / Failed

    public string Method { get; set; }

    public string Message { get; set; }

    public BankResponseModel BankResponseModel { get; set; }

    public IBoxResponseModel IBoxResponseModel { get; set; }
}
