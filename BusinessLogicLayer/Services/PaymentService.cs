using System.Net;
using System.Net.Http.Json;
using BusinessLogicLayer.Models.Payment;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Exceptions;
using GameStore.Shared.FileSystem;
using GameStore.Shared.Stubs;
using OrderStatus = GameStore.Shared.Enums.OrderStatus;
using PaymentOptions = GameStore.Shared.Enums.PaymentEnums.PaymentOptions;

namespace BusinessLogicLayer.Services;

public class PaymentService : IPaymentService
{
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

    public IEnumerable<PaymentMethodResponseModel> GetPaymentMethods()
    {
        var paymentMethods = new List<PaymentMethodResponseModel>
    {
        new PaymentMethodResponseModel
        {
            ImageUrl = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
            Title = "Bank",
            Description = "Payment via bank account. You will receive a PDF invoice for payment.",
        },
        new PaymentMethodResponseModel
        {
            ImageUrl = "https://green-api.volia.com/media/6407a7b23eb69_ibox.jpg",
            Title = "IBox terminal",
            Description = "Payment via the IBox self-service terminal.",
        },
        new PaymentMethodResponseModel
        {
            ImageUrl = "https://cdn-icons-png.flaticon.com/512/349/349221.png",
            Title = "Visa",
            Description = "Payment by Visa bank card.",
        },
    };

        return paymentMethods;
    }

    public async Task<PaymentResponseModel> ProcessPayment(PaymentRequestModel request)
    {
        ValidateRequest(request);

        var order = await GetActiveOrderAsync();
        var total = CalculateOrderTotal(order);

        return request.Method switch
        {
            PaymentOptions.Visa => await ProcessVisaPayment(request.Model!, order, total),
            PaymentOptions.IBoxTerminal => await ProcessIBoxPayment(order, total),
            PaymentOptions.Bank => await ProcessBankPayment(order, total),
            _ => throw new NotSupportedException($"Unsupported payment method: {request.Method}"),
        };
    }

    private async Task<PaymentResponseModel> ProcessVisaPayment(
        VisaPaymentModel model,
        OrderEntity order,
        decimal total)
    {
        model.TotalAmount = total;

        var response = await ExecuteHttpPayment(
            order,
            () => _client.PostAsJsonAsync("/api/payments/visa", model));

        return BuildPaymentResponseAsync(
            response,
            PaymentOptions.Visa);
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
            PaymentOptions.IBoxTerminal);

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

        if (fileBytes == null || fileBytes.Length == 0)
        {
            throw new InvoiceGenerationException("Failed to generate PDF invoice.");
        }

        var path = Path.Combine("Invoices", $"invoice_{order.Id}.pdf");
        await _fileSystem.WriteFileAsync(path, fileBytes);

        await SetOrderStatusAsync(order, OrderStatus.Paid);

        return new PaymentResponseModel
        {
            Method = PaymentOptions.Bank,
            BankResponseModel = new BankResponseModel
            {
                FileBytes = fileBytes,
            },
        };
    }

    // Core infra logic

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
    PaymentOptions method)
    {
        PaymentResponseModel result = response.StatusCode == HttpStatusCode.PaymentRequired
            ? throw new BadRequestException("Not enough funds to complete the payment.")
            : new PaymentResponseModel
            {
                Method = method,
            };
        return result;
    }

    private async Task SetOrderStatusAsync(OrderEntity order, OrderStatus status)
    {
        order.Status = status;
        await _unitOfWork.SaveAsync();
    }

    // Helpers

    private static void ValidateRequest(PaymentRequestModel request)
    {
        if (!Enum.IsDefined(typeof(PaymentOptions), request.Method))
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