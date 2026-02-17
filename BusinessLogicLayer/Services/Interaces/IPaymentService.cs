using BusinessLogicLayer.Models.Payment;

namespace BusinessLogicLayer.Services.Interaces;

public interface IPaymentService
{
    /// <summary>
    /// Основний метод обробки платежів.
    /// Вибір методу (Visa, IBox, Bank) відбувається всередині сервісу.
    /// </summary>
    Task<PaymentResponseModel> ProcessPayment(PaymentRequestModel request);

    /// <summary>
    /// Позначає замовлення як оплачене.
    /// </summary>
    Task MarkOrderPaid(Guid orderId);

    /// <summary>
    /// Позначає замовлення як скасоване.
    /// </summary>
    Task MarkOrderCancelled(Guid orderId);
}


//using BusinessLogicLayer.Models;

//namespace BusinessLogicLayer.Services.Interaces;

//public interface IPaymentService
//{
//    Task<PaymentResponseModel> PayWithVisa(VisaPaymentModel payment);

//    Task<PaymentResponseModel> PayWithIBox(IBoxPaymentModel payment);

//    Task<PaymentResponseModel> PayWithBank(BankPaymentModel payment);

//    Task MarkOrderPaid(Guid orderId);

//    Task MarkOrderCancelled(Guid orderId);
//}
