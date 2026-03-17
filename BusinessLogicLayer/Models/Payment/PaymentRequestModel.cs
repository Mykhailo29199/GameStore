namespace BusinessLogicLayer.Models.Payment;

public class PaymentRequestModel
{
    public string Method { get; set; }

    public VisaPaymentModel? Model { get; set; }
}
