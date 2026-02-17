namespace BusinessLogicLayer.Models.Payment;

public class IBoxResponseModel
{
    public Guid OrderId { get; set; }

    // Використовується для Bank/Visa
    public Guid? UserId { get; set; }

    public decimal Sum { get; set; }

    public DateTime PaymentDate { get; set; }
}
