using BusinessLogicLayer.Models.UserModels;

namespace BusinessLogicLayer.Services.Interaces;

public interface IUserService
{
    Task<AuthenticatedUserModel> LoginAsync(LoginModel model);

    /// <summary>
    /// Self-registration: creates a user with the User role only. Other roles are assigned by an administrator.
    /// </summary>
    Task<AuthenticatedUserModel> RegisterAsync(string name, string password);

    Task<List<UserModel>> GetUsersAsync();

    Task<UserModel> GetUserByIdAsync(Guid id);

    Task<List<RoleModel>> GetUserRolesAsync(Guid id);

    Task AddUserAsync(UserModel user, IReadOnlyCollection<Guid> roleIds, string password);

    Task UpdateUserAsync(UserModel user, IReadOnlyCollection<Guid> roleIds, string? password);

    Task DeleteUserAsync(Guid id);
}
