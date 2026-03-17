namespace BusinessLogicLayer.Models.Payment;

public class BoxPaymentModel
{
    public decimal TransactionAmount { get; set; }

    public Guid AccountNumber { get; set; }

    public Guid InvoiceNumber { get; set; }
}