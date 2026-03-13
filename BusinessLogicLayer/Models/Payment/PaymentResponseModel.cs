namespace BusinessLogicLayer.Models.Payment;

public class PaymentResponseModel
{
    public string Method { get; set; }

    public BankResponseModel BankResponseModel { get; set; }

    public BoxResponseModel IBoxResponseModel { get; set; }
}
