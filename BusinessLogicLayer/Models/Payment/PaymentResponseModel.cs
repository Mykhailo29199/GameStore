using GameStore.Shared.Enums.PaymentEnums;

namespace BusinessLogicLayer.Models.Payment;

public class PaymentResponseModel
{
    public PaymentOptions Method { get; set; }

    public BankResponseModel BankResponseModel { get; set; }

    public BoxResponseModel IBoxResponseModel { get; set; }
}
