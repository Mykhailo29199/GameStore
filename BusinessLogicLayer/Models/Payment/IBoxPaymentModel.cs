namespace BusinessLogicLayer.Models.Payment;

public class IBoxPaymentModel
{
    public decimal TransactionAmount { get; set; }

    public Guid AccountNumber { get; set; }

    public Guid InvoiceNumber { get; set; }
}