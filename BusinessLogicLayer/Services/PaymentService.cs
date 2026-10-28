using System.Net;
using System.Net.Http.Json;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Models.Payment;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
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
    private readonly ISqlServerUnitOfWork _unitOfWork;
    private readonly IProductCatalog _productCatalog;

    public PaymentService(
        IHttpClientFactory httpClientFactory,
        ISqlServerUnitOfWork unitOfWork,
        IFileSystem fileSystem,
        IProductCatalog productCatalog)
    {
        _client = httpClientFactory.CreateClient("PaymentService");
        _unitOfWork = unitOfWork;
        _fileSystem = fileSystem;
        _productCatalog = productCatalog;
    }

    public IEnumerable<PaymentMethodResponseModel> GetPaymentMethods()
    {
        return
        [
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
        ];
    }

    public async Task<PaymentResponseModel> ProcessPayment(PaymentRequestModel request)
    {
        ValidateRequest(request);

        var order = await GetActiveOrderAsync();

        // Stock is checked before any money is taken.
        await GetProductsInStockAsync(order);

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

        var soldProducts = status == OrderStatus.Paid ? await TakeUnitsFromStockAsync(order) : [];

        await _unitOfWork.SaveAsync();

        // Northwind gets the saved value only after the Game Store database is saved, so a failed save changes nothing there.
        await _productCatalog.SyncUnitsInStockAsync(soldProducts);
    }

    private async Task<List<ProductEntity>> TakeUnitsFromStockAsync(OrderEntity order)
    {
        var products = await GetProductsInStockAsync(order);

        foreach (var (product, quantity) in products)
        {
            product.UnitsInStock -= quantity;
        }

        return products.Select(p => p.Product).ToList();
    }

    /// <summary>
    /// Returns the ordered games with their quantities. Throws when a game has fewer units in stock than ordered.
    /// </summary>
    private async Task<List<(ProductEntity Product, int Quantity)>> GetProductsInStockAsync(OrderEntity order)
    {
        var products = new List<(ProductEntity Product, int Quantity)>();

        foreach (var orderDetails in order.OrderDetails)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(orderDetails.ProductId);

            if (product is null)
            {
                continue;
            }

            if (product.UnitsInStock < orderDetails.Quantity)
            {
                throw new BadRequestException($"Not enough units in stock for game '{product.ProductName}'.");
            }

            products.Add((product, orderDetails.Quantity));
        }

        return products;
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
        return await _unitOfWork.OrderRepository.GetActiveCartAsync(CustomerStub.StubCustomerId)
            ?? throw new NotFoundException("Active cart not found");
    }

    private static decimal CalculateOrderTotal(OrderEntity order)
    {
        return OrderTotalCalculator.Total(
            order.OrderDetails.Select(og => (og.UnitPrice, og.Quantity, og.Discount)));
    }
}