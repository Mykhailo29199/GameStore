//using System.Net.Http.Json;
//using BusinessLogicLayer.Models;
//using BusinessLogicLayer.Models.Payment;
//using BusinessLogicLayer.Services.Interaces;
//using GameStore.Shared.FileSystem;
//using MigraDocCore.Rendering;

//namespace BusinessLogicLayer.Services;

//public class PaymentService(IHttpClientFactory httpClientFactory, IFileSystem fileSystem) : IPaymentService
//{
//    private readonly HttpClient _client = httpClientFactory.CreateClient("PaymentService");
//    private readonly IFileSystem _fileSystem = fileSystem;

//    public async Task<BasePaymentResponseModel> ProcessPayment(PaymentRequestModel request)
//    {
//        if (string.IsNullOrWhiteSpace(request.Method))
//        {
//            throw new ArgumentException("Payment method is required.");
//        }

//        return request.Method switch
//        {
//            "Visa" => await PayWithVisa(request.Model!, request.OrderId!),
//            "IBox terminal" => await PayWithIBox(request.IBoxModel!, request.OrderId!),
//            "Bank" => await PayWithBank(request.BankModel!, request.OrderId!),
//            _ => throw new NotSupportedException($"Unsupported payment method: {request.Method}"),
//        };
//    }

//    public async Task MarkOrderPaid(Guid orderId)
//    {
//        await _client.PostAsync($"/api/orders/{orderId}/mark-paid", null);
//    }

//    public async Task MarkOrderCancelled(Guid orderId)
//    {
//        await _client.PostAsync($"/api/orders/{orderId}/mark-cancelled", null);
//    }

//    private async Task<VisaPaymentResponseModel> PayWithVisa(VisaPaymentModel payment)
//    {
//        var response = await _client.PostAsJsonAsync("/api/payments/visa", payment);

//        return new VisaPaymentResponseModel
//        {
//            Method = "Visa",
//            Status = response.IsSuccessStatusCode ? "Success" : "Failed",
//        };
//    }

//    private async Task<IBoxPaymentResponseDTO> PayWithIBox(IBoxPaymentModel payment, Guid orderId)
//    {
//        var response = await _client.PostAsJsonAsync("/api/payments/ibox", payment);

//        return new IBoxPaymentResponseDTO
//        {
//            Method = "IBox terminal",
//            Status = response.IsSuccessStatusCode ? "Success" : "Failed",
//            UserId = payment.UserId,
//            OrderId = orderId,
//            PaymentDate = DateTime.UtcNow,
//            Sum = payment.TransactionAmount,
//        };
//    }

//    private async Task<BankPaymentResponseDTO> PayWithBank(BankPaymentModel payment, Guid orderId)
//    {
//        var fileBytes = GeneratePdfInvoice(payment, orderId);

//        var dir = "Invoices";
//        var fileName = $"invoice_{orderId}.pdf";
//        var filePath = Path.Combine(dir, fileName);

//        await _fileSystem.WriteFileAsync(filePath, fileBytes);

//        return new BankPaymentResponseDTO
//        {
//            Method = "Bank",
//            Status = "Success",
//            UserId = payment.UserId,
//            OrderId = orderId,
//            CreationDate = payment.CreationDate,
//            ValidUntil = payment.ValidUntil,
//            Sum = payment.Sum,
//            FileBytes = fileBytes,
//        };
//    }

//    private static byte[] GeneratePdfInvoice(BankPaymentModel payment, Guid orderId)
//    {
//        var document = new MigraDocCore.DocumentObjectModel.Document();
//        var section = document.AddSection();

//        var header = section.AddParagraph($"Invoice #{orderId}");
//        header.Format.Font.Size = 16;
//        header.Format.Font.Bold = true;
//        header.Format.SpaceAfter = "1cm";

//        section.AddParagraph($"User ID: {payment.UserId}");
//        section.AddParagraph($"Order ID: {orderId}");
//        section.AddParagraph($"Creation Date: {payment.CreationDate:yyyy-MM-dd HH:mm:ss}");
//        section.AddParagraph($"Valid Until: {payment.ValidUntil:yyyy-MM-dd HH:mm:ss}");
//        section.AddParagraph($"Sum: {payment.Sum}");

//        var renderer = new PdfDocumentRenderer(unicode: true)
//        {
//            Document = document,
//        };

//        renderer.RenderDocument();

//        using var ms = new MemoryStream();
//        renderer.PdfDocument.Save(ms, false);
//        return ms.ToArray();
//    }
// }
using System.Net.Http.Json;
using BusinessLogicLayer.Models.Payment;
using BusinessLogicLayer.Services.Interaces;
using GameStore.Shared.FileSystem;
using MigraDocCore.Rendering;

namespace BusinessLogicLayer.Services;

public class PaymentService(IHttpClientFactory httpClientFactory, IFileSystem fileSystem) : IPaymentService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("PaymentService");
    private readonly IFileSystem _fileSystem = fileSystem;

    public async Task<PaymentResponseModel> ProcessPayment(PaymentRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Method))
        {
            throw new ArgumentException("Payment method is required.");
        }

        switch (request.Method)
        {
            case "Visa":
                return await PayWithVisa(request.Model!);

            case "IBox terminal":
                return await PayWithIBox(request.IBoxModel!, request.OrderId!);

            case "Bank":
                return await PayWithBank(request.BankModel!, request.OrderId!);

            default:
                throw new NotSupportedException($"Unsupported payment method: {request.Method}");
        }
    }

    public async Task MarkOrderPaid(Guid orderId)
    {
        await _client.PostAsync($"/api/orders/{orderId}/mark-paid", null);
    }

    public async Task MarkOrderCancelled(Guid orderId)
    {
        await _client.PostAsync($"/api/orders/{orderId}/mark-cancelled", null);
    }

    private async Task<PaymentResponseModel> PayWithVisa(VisaPaymentModel payment)
    {
        var response = await _client.PostAsJsonAsync("/api/payments/visa", payment);

        if (response.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
        {
            return new PaymentResponseModel
            {
                Status = "Failed",
                Method = "Visa",
                Message = "Недостатньо коштів для оплати Visa",
            };
        }

        return new PaymentResponseModel
        {
            Status = response.IsSuccessStatusCode ? "Success" : "Failed",
            Method = "Visa",
        };
    }

    private async Task<PaymentResponseModel> PayWithIBox(IBoxPaymentModel payment, Guid orderId)
    {
        var response = await _client.PostAsJsonAsync("/api/payments/ibox", payment);

        if (response.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
        {
            return new PaymentResponseModel
            {
                Status = "Failed",
                Method = "IBox terminal",
                Message = "Недостатньо коштів для оплати через IBox",
            };
        }

        IBoxResponseModel responseModel = new()
        {
            OrderId = orderId,
            UserId = Guid.NewGuid(),
            PaymentDate = DateTime.UtcNow,
            Sum = payment.TransactionAmount,
        };

        return new PaymentResponseModel
        {
            Status = response.IsSuccessStatusCode ? "Success" : "Failed",
            Method = "IBox terminal",
            IBoxResponseModel = responseModel,
        };
    }

    private async Task<PaymentResponseModel> PayWithBank(BankPaymentModel payment, Guid orderId)
    {
        var fileBytes = GeneratePdfInvoice(payment, orderId);

        var dir = "Invoices";
        var fileName = $"invoice_{orderId}.pdf";
        var filePath = Path.Combine(dir, fileName);

        await _fileSystem.WriteFileAsync(filePath, fileBytes);

        BankResponseModel responseModel = new()
        {
            FileBytes = fileBytes,
        };

        return new PaymentResponseModel
        {
            Status = "Success",
            Method = "Bank",
            BankResponseModel = responseModel,
        };
    }

    private static byte[] GeneratePdfInvoice(BankPaymentModel payment, Guid orderId)
    {
        var document = new MigraDocCore.DocumentObjectModel.Document();
        var section = document.AddSection();

        // Заголовок
        var header = section.AddParagraph($"Invoice #{orderId}");
        header.Format.Font.Size = 16;
        header.Format.Font.Bold = true;
        header.Format.SpaceAfter = "1cm";

        // Основний текст
        section.AddParagraph($"User ID: {payment.UserId}");
        section.AddParagraph($"Order ID: {orderId}");
        section.AddParagraph($"Creation Date: {payment.CreationDate:yyyy-MM-dd HH:mm:ss}");
        section.AddParagraph($"Valid Until: {payment.ValidUntil:yyyy-MM-dd HH:mm:ss}");
        section.AddParagraph($"Sum: {payment.Sum}");

        // Рендеринг у PDF
        var renderer = new PdfDocumentRenderer(unicode: true)
        {
            Document = document,
        };

        renderer.RenderDocument();

        using var ms = new MemoryStream();
        renderer.PdfDocument.Save(ms, false);
        return ms.ToArray();
    }
}