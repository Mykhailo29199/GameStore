using BusinessLogicLayer.Models.Payment;

namespace BusinessLogicLayer.Services.Interaces;

public interface IPaymentService
{
    Task<PaymentResponseModel> ProcessPayment(PaymentRequestModel request);

    Task MarkOrderPaid(Guid orderId);

    Task MarkOrderCancelled(Guid orderId);
}