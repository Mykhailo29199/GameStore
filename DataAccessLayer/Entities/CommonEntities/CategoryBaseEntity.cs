using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.CommonEntities;

public class CategoryBaseEntity
{
    public int? CategoryID { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? Picture { get; set; }
}