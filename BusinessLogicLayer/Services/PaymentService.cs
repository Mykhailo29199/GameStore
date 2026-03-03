using System.Net;
using System.Net.Http.Json;
using BusinessLogicLayer.Models.Payment;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Enums;
using GameStore.Shared.Exceptions;
using GameStore.Shared.FileSystem;
using GameStore.Shared.Stubs;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;

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

    public Task MarkOrderPaid(Guid orderId) =>
        _client.PostAsync($"/api/orders/{orderId}/mark-paid", null);

    public Task MarkOrderCancelled(Guid orderId) =>
        _client.PostAsync($"/api/orders/{orderId}/mark-cancelled", null);

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

        return await BuildPaymentResponseAsync(
            response,
            order,
            VisaMethod,
            "Insufficient funds for Visa payment.");
    }

    private async Task<PaymentResponseModel> ProcessIBoxPayment(
        OrderEntity order,
        decimal total)
    {
        var model = new IBoxPaymentModel
        {
            AccountNumber = CustomerStub.StubCustomerId,
            TransactionAmount = total,
            InvoiceNumber = order.Id,
        };

        var response = await ExecuteHttpPayment(
            order,
            () => _client.PostAsJsonAsync("/api/payments/ibox", model));

        var result = await BuildPaymentResponseAsync(
            response,
            order,
            IBoxMethod,
            "Not enough funds to complete the payment via IBox.");

        if (response.IsSuccessStatusCode)
        {
            result.IBoxResponseModel = new IBoxResponseModel
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

        var fileBytes = GeneratePdfInvoice(order.Id, total);

        var path = Path.Combine("Invoices", $"invoice_{order.Id}.pdf");
        await _fileSystem.WriteFileAsync(path, fileBytes);

        return new PaymentResponseModel
        {
            Status = "Success",
            Method = BankMethod,
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

        return await httpCall();
    }

    private async Task<PaymentResponseModel> BuildPaymentResponseAsync(
     HttpResponseMessage response,
     OrderEntity order,
     string method,
     string insufficientFundsMessage)
    {
        var newStatus = response.IsSuccessStatusCode
            ? OrderStatus.Paid
            : OrderStatus.Cancelled;

        await SetOrderStatusAsync(order, newStatus);

        if (response.StatusCode == HttpStatusCode.PaymentRequired)
        {
            return new PaymentResponseModel
            {
                Status = "Failed",
                Method = method,
                Message = insufficientFundsMessage,
            };
        }

        return new PaymentResponseModel
        {
            Status = response.IsSuccessStatusCode ? "Success" : "Failed",
            Method = method,
        };
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
            throw new ArgumentException("Payment method is required.");
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

    private static byte[] GeneratePdfInvoice(Guid orderId, decimal total)
    {
        var document = new Document();
        var section = document.AddSection();

        var header = section.AddParagraph($"Invoice #{orderId}");
        header.Format.Font.Size = 16;
        header.Format.Font.Bold = true;
        header.Format.SpaceAfter = "1cm";

        section.AddParagraph($"User ID: {CustomerStub.StubCustomerId}");
        section.AddParagraph($"Order ID: {orderId}");
        section.AddParagraph($"Creation Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        section.AddParagraph($"Valid Until: {DateTime.UtcNow.AddDays(7):yyyy-MM-dd HH:mm:ss}");
        section.AddParagraph($"Sum: {total}");

        var renderer = new PdfDocumentRenderer(unicode: true)
        {
            Document = document,
        };

        renderer.RenderDocument();

        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);

        return stream.ToArray();
    }
}