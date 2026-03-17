namespace GameStore.Shared.Exceptions;

public class InvoiceGenerationException : Exception
{
    public InvoiceGenerationException(string message)
        : base(message)
    {
    }
}
