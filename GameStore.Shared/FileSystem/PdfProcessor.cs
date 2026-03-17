using GameStore.Shared.Stubs;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;

namespace GameStore.Shared.FileSystem;

public class PdfProcessor
{
    public static byte[] GeneratePdfInvoice(Guid orderId, decimal total)
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
