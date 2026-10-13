using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.CommonEntities;

public abstract class RegionBaseEntity
{
    public int? RegionID { get; set; }

    [Required]
    [MaxLength(50)]
    public string? RegionDescription { get; set; }
}