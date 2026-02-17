namespace BusinessLogicLayer.Models.Payment;

public class PaymentRequestModel
{
    public string Method { get; set; }

    public Guid OrderId { get; set; }

    public VisaPaymentModel? Model { get; set; }

    public IBoxPaymentModel? IBoxModel { get; set; }

    public BankPaymentModel? BankModel { get; set; }
}
