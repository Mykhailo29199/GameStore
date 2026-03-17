using BusinessLogicLayer.Models.Payment;

namespace BusinessLogicLayer.Services.Interaces;

public interface IPaymentService
{
    Task<PaymentResponseModel> ProcessPayment(PaymentRequestModel request);

    IEnumerable<PaymentMethodResponseModel> GetPaymentMethods();
}