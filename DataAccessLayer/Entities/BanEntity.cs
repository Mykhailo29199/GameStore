using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Entities;

[Table("BanEntity")]
public class BanEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserName { get; set; }

    public DateTime? ExpireAt { get; set; }
}
