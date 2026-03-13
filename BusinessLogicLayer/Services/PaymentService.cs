using System.Net;
using System.Net.Http.Json;
using BusinessLogicLayer.Models.Payment;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Exceptions;
using GameStore.Shared.Extensions;
using GameStore.Shared.FileSystem;
using GameStore.Shared.Stubs;
using OrderStatus = GameStore.Shared.Enums.OrderStatus;
using PaymentOptions = GameStore.Shared.Enums.PaymentOptions;

namespace BusinessLogicLayer.Services;

public class PaymentService : IPaymentService
{
    private const string VisaMethod = "Visa";
    private const string IBoxMethod = "IBox terminal";
    private const string BankMethod = "Bank";
    private readonly HttpClient _client;
    private readonly IFileSystem _fileSystem;
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork;

    public PaymentService(
        IHttpClientFactory httpClientFactory,
        IUnitOfWork<GameStoreContext> unitOfWork,
        IFileSystem fileSystem)
    {
        _client = httpClientFactory.CreateClient("PaymentService");
        _unitOfWork = unitOfWork;
        _fileSystem = fileSystem;
    }

    public async Task<PaymentResponseModel> ProcessPayment(PaymentRequestModel request)
    {
        ValidateRequest(request);

        var order = await GetActiveOrderAsync();
        var total = CalculateOrderTotal(order);

        return request.Method switch
        {
            VisaMethod => await ProcessVisaPayment(request.Model!, order, total),
            IBoxMethod => await ProcessIBoxPayment(order, total),
            BankMethod => await ProcessBankPayment(order, total),
            _ => throw new NotSupportedException($"Unsupported payment method: {request.Method}"),
        };
    }

    // ============================
    // PAYMENT METHODS
    // ============================

    private async Task<PaymentResponseModel> ProcessVisaPayment(
        VisaPaymentModel model,
        OrderEntity order,
        decimal total)
    {
        model.TransactionAmount = total;

        var response = await ExecuteHttpPayment(
            order,
            () => _client.PostAsJsonAsync("/api/payments/visa", model));

        return BuildPaymentResponseAsync(
            response,
            PaymentOptions.Visa.GetDescription(),
            "Insufficient funds for Visa payment.");
    }

    private async Task<PaymentResponseModel> ProcessIBoxPayment(
        OrderEntity order,
        decimal total)
    {
        var model = new BoxPaymentModel
        {
            AccountNumber = CustomerStub.StubCustomerId,
            TransactionAmount = total,
            InvoiceNumber = order.Id,
        };

        var response = await ExecuteHttpPayment(
            order,
            () => _client.PostAsJsonAsync("/api/payments/ibox", model));

        var result = BuildPaymentResponseAsync(
            response,
            PaymentOptions.IBoxTerminal.GetDescription(),
            "Not enough funds to complete the payment via IBox.");

        if (response.IsSuccessStatusCode)
        {
            result.IBoxResponseModel = new BoxResponseModel
            {
                OrderId = order.Id,
                UserId = Guid.NewGuid(),
                PaymentDate = DateTime.UtcNow,
                Sum = total,
            };
        }

        return result;
    }

    private async Task<PaymentResponseModel> ProcessBankPayment(
        OrderEntity order,
        decimal total)
    {
        await SetOrderStatusAsync(order, OrderStatus.Checkout);

        var fileBytes = PdfProcessor.GeneratePdfInvoice(order.Id, total);

        var path = Path.Combine("Invoices", $"invoice_{order.Id}.pdf");
        await _fileSystem.WriteFileAsync(path, fileBytes);

        await SetOrderStatusAsync(order, OrderStatus.Paid);

        return new PaymentResponseModel
        {
            Status = "Success",
            Method = PaymentOptions.Bank.GetDescription(),
            BankResponseModel = new BankResponseModel
            {
                FileBytes = fileBytes,
            },
        };
    }

    // ============================
    // CORE INFRA LOGIC
    // ============================

    private async Task<HttpResponseMessage> ExecuteHttpPayment(
     OrderEntity order,
     Func<Task<HttpResponseMessage>> httpCall)
    {
        await SetOrderStatusAsync(order, OrderStatus.Checkout);
        var result = await httpCall();
        if (result.IsSuccessStatusCode)
        {
            await SetOrderStatusAsync(order, OrderStatus.Paid);
        }
        else
        {
            await SetOrderStatusAsync(order, OrderStatus.Cancelled);
        }

        return result;
    }

    private static PaymentResponseModel BuildPaymentResponseAsync(
    HttpResponseMessage response,
    string method,
    string insufficientFundsMessage)
    {
        var status = response.IsSuccessStatusCode ? "Success" : "Failed";

        PaymentResponseModel result = response.StatusCode == HttpStatusCode.PaymentRequired
            ? new PaymentResponseModel
            {
                Status = "Failed",
                Method = method,
                Message = insufficientFundsMessage,
            }
            : new PaymentResponseModel
            {
                Status = status,
                Method = method,
            };
        return result;
    }

    private async Task SetOrderStatusAsync(OrderEntity order, OrderStatus status)
    {
        order.Status = status;
        await _unitOfWork.SaveAsync();
    }

    // ============================
    // HELPERS
    // ============================

    private static void ValidateRequest(PaymentRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Method))
        {
            throw new BadRequestException("Payment method is required.");
        }
    }

    private async Task<OrderEntity> GetActiveOrderAsync()
    {
        return await _unitOfWork.OrderRepository
                   .GetActiveCartAsync(CustomerStub.StubCustomerId)
               ?? throw new NotFoundException("Active cart not found");
    }

    private static decimal CalculateOrderTotal(OrderEntity order)
    {
        return order.OrderGames.Sum(og =>
        {
            var price = (decimal)og.Price;
            var discount = og.Discount ?? 0;
            var discounted = price * (1 - (discount / 100m));
            return discounted * og.Quantity;
        });
    }
}