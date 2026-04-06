using GameStore.Shared.Enums.PaymentEnums;

namespace BusinessLogicLayer.Models.Payment;

public class PaymentRequestModel
{
    public PaymentOptions Method { get; set; }

    public VisaPaymentModel? Model { get; set; }
}
