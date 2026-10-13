using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.CommonEntities;

public abstract class CustomerBaseEntity
{
    public string? CustomerID { get; set; }

    [MaxLength(100)]
    public string? CompanyName { get; set; }

    [MaxLength(100)]
    public string? ContactName { get; set; }

    public string? ContactTitle { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Region { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    [MaxLength(50)]
    public string? Field11 { get; set; }
}