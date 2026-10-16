using BusinessLogicLayer.Models.UserModels;

namespace BusinessLogicLayer.Services.Interaces;

public interface IRoleService
{
    Task<List<RoleModel>> GetRolesAsync();

    Task<RoleModel> GetRoleByIdAsync(Guid id);

    IReadOnlyList<string> GetPermissions();

    Task<List<string>> GetRolePermissionsAsync(Guid id);

    Task AddRoleAsync(RoleModel role, IReadOnlyCollection<string> permissions);

    Task UpdateRoleAsync(RoleModel role, IReadOnlyCollection<string> permissions);

    Task DeleteRoleAsync(Guid id);
}
