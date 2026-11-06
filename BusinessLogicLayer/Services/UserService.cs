using BusinessLogicLayer.Models.UserModels;
using BusinessLogicLayer.Security;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Authorization;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class UserService(
    ISqlServerUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IExternalAuthClient externalAuthClient,
    ICurrentUser currentUser) : IUserService
{
    private const string InvalidCredentialsMessage = "Invalid login or password.";

    public async Task<AuthenticatedUserModel> LoginAsync(LoginModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Login) || string.IsNullOrEmpty(model.Password))
        {
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        var user = model.InternalAuth
            ? await LoginInternalAsync(model)
            : await LoginExternalAsync(model);

        var permissions = await unitOfWork.RoleRepository.GetPermissionsAsync(user.UserRoles.Select(ur => ur.RoleId).ToList());

        return new AuthenticatedUserModel
        {
            Id = user.Id,
            Name = user.Name,
            Permissions = permissions,
        };
    }

    public async Task<AuthenticatedUserModel> RegisterAsync(string name, string password)
    {
        await AddUserAsync(new UserModel { Name = name }, [DefaultRoles.UserId], password);

        return await LoginAsync(new LoginModel { Login = name.Trim(), Password = password, InternalAuth = true });
    }

    public async Task<List<UserModel>> GetUsersAsync()
    {
        var users = await unitOfWork.UserRepository.GetAllAsync();

        return users.Select(ToModel).ToList();
    }

    public async Task<UserModel> GetUserByIdAsync(Guid id)
    {
        return ToModel(await GetExistingUserAsync(id));
    }

    public async Task<List<RoleModel>> GetUserRolesAsync(Guid id)
    {
        var user = await GetExistingUserAsync(id);

        return user.UserRoles
            .Select(ur => new RoleModel { Id = ur.Role.Id, Name = ur.Role.Name })
            .ToList();
    }

    public async Task AddUserAsync(UserModel user, IReadOnlyCollection<Guid> roleIds, string password)
    {
        if (string.IsNullOrWhiteSpace(user.Name))
        {
            throw new BadRequestException("User name is required.");
        }

        if (string.IsNullOrEmpty(password))
        {
            throw new BadRequestException("Password is required.");
        }

        await EnsureNameIsFreeAsync(user.Name, null);
        await EnsureRolesExistAsync(roleIds);

        var entity = new UserEntity
        {
            Name = user.Name.Trim(),
            PasswordHash = passwordHasher.Hash(password),
            UserRoles = roleIds.Distinct().Select(roleId => new UserRoleEntity { RoleId = roleId }).ToList(),
        };

        await unitOfWork.UserRepository.AddAsync(entity);
        await unitOfWork.SaveAsync();
    }

    public async Task UpdateUserAsync(UserModel user, IReadOnlyCollection<Guid> roleIds, string? password)
    {
        var entity = await GetExistingUserAsync(user.Id);

        if (string.IsNullOrWhiteSpace(user.Name))
        {
            throw new BadRequestException("User name is required.");
        }

        await EnsureNameIsFreeAsync(user.Name, user.Id);
        await EnsureRolesExistAsync(roleIds);

        entity.Name = user.Name.Trim();

        if (!string.IsNullOrEmpty(password))
        {
            entity.PasswordHash = passwordHasher.Hash(password);
        }

        var newRoleIds = roleIds.Distinct().ToList();

        foreach (var removed in entity.UserRoles.Where(ur => !newRoleIds.Contains(ur.RoleId)).ToList())
        {
            entity.UserRoles.Remove(removed);
        }

        foreach (var roleId in newRoleIds.Where(id => entity.UserRoles.All(ur => ur.RoleId != id)))
        {
            entity.UserRoles.Add(new UserRoleEntity { UserId = entity.Id, RoleId = roleId });
        }

        await unitOfWork.SaveAsync();
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var entity = await GetExistingUserAsync(id);

        if (currentUser.Id == id)
        {
            throw new BadRequestException("You cannot delete your own account.");
        }

        await unitOfWork.UserRepository.DeleteAsync(entity);
        await unitOfWork.SaveAsync();
    }

    private static UserModel ToModel(UserEntity user)
    {
        return new UserModel { Id = user.Id, Name = user.Name };
    }

    private async Task<UserEntity> LoginInternalAsync(LoginModel model)
    {
        var user = await unitOfWork.UserRepository.GetByNameWithRolesAsync(model.Login);

        return user is not null && passwordHasher.Verify(model.Password, user.PasswordHash)
            ? user
            : throw new UnauthorizedException(InvalidCredentialsMessage);
    }

    /// <summary>
    /// Users confirmed by the external authorization service get a local account with the User role on first login.
    /// </summary>
    private async Task<UserEntity> LoginExternalAsync(LoginModel model)
    {
        if (!await externalAuthClient.AuthenticateAsync(model.Login, model.Password))
        {
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        var user = await unitOfWork.UserRepository.GetByNameWithRolesAsync(model.Login);

        if (user is not null)
        {
            return user;
        }

        user = new UserEntity
        {
            Name = model.Login,
            UserRoles = [new UserRoleEntity { RoleId = DefaultRoles.UserId }],
        };

        await unitOfWork.UserRepository.AddAsync(user);
        await unitOfWork.SaveAsync();

        return user;
    }

    private async Task<UserEntity> GetExistingUserAsync(Guid id)
    {
        return await unitOfWork.UserRepository.GetWithRolesAsync(id)
            ?? throw new NotFoundException($"User with id '{id}' does not exist.");
    }

    private async Task EnsureNameIsFreeAsync(string name, Guid? userId)
    {
        var trimmedName = name.Trim();
        var existing = await unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.Name == trimmedName);

        if (existing is not null && existing.Id != userId)
        {
            throw new BadRequestException($"User with name '{trimmedName}' already exists.");
        }
    }

    private async Task EnsureRolesExistAsync(IReadOnlyCollection<Guid> roleIds)
    {
        var existingIds = (await unitOfWork.RoleRepository.GetAllAsync()).Select(r => r.Id).ToHashSet();
        var missing = roleIds.Where(id => !existingIds.Contains(id)).ToList();

        if (missing.Count > 0)
        {
            throw new NotFoundException($"Role with id '{missing[0]}' does not exist.");
        }
    }
}
