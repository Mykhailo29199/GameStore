namespace DataAccessLayer.Entities.CommonEntities;

public abstract class ShipperBaseEntity
{
    public int? ShipperID { get; set; }

    public string? CompanyName { get; set; }

    public string? Phone { get; set; }
}