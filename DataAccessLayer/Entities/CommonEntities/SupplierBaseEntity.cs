using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.CommonEntities;

public class SupplierBaseEntity
{
    public int? SupplierID { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; }

    [MaxLength(200)]
    public string? ContactName { get; set; }

    [MaxLength(200)]
    public string? ContactTitle { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Region { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string? Fax { get; set; }

    [MaxLength(333)]
    public string? HomePage { get; set; }
}
