using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities.GameStoreSQLEntities;

public class UserEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the password hash. Empty for users that are authenticated only by the external authorization service.
    /// </summary>
    public string? PasswordHash { get; set; }

    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}
