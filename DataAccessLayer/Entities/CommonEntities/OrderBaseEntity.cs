using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.CommonEntities;

public abstract class OrderBaseEntity
{
    public DateTime? OrderDate { get; set; }

    public DateTime? RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public int? ShipVia { get; set; }

    public double? Freight { get; set; }

    [MaxLength(200)]
    public string? ShipName { get; set; }

    public string? ShipRegion { get; set; }

    public string? ShipPostalCode { get; set; }

    public string? ShipCountry { get; set; }

    public string? Field14 { get; set; }
}