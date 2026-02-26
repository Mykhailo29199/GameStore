namespace BusinessLogicLayer.Models.Payment;

public class IBoxResponseModel
{
    public Guid OrderId { get; set; }

    public Guid? UserId { get; set; }

    public decimal Sum { get; set; }

    public DateTime PaymentDate { get; set; }
}
