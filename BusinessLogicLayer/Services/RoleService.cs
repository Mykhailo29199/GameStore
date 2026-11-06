using BusinessLogicLayer.Models.UserModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Authorization;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class RoleService(ISqlServerUnitOfWork unitOfWork) : IRoleService
{
    public async Task<List<RoleModel>> GetRolesAsync()
    {
        var roles = await unitOfWork.RoleRepository.GetAllAsync();

        return roles.Select(ToModel).ToList();
    }

    public async Task<RoleModel> GetRoleByIdAsync(Guid id)
    {
        return ToModel(await GetExistingRoleAsync(id));
    }

    public IReadOnlyList<string> GetPermissions()
    {
        return Permissions.All;
    }

    public async Task<List<string>> GetRolePermissionsAsync(Guid id)
    {
        var role = await GetExistingRoleAsync(id);

        return role.Permissions.Select(p => p.Permission).ToList();
    }

    public async Task AddRoleAsync(RoleModel role, IReadOnlyCollection<string> permissions)
    {
        var name = await ValidateAsync(role, permissions);

        var entity = new RoleEntity
        {
            Name = name,
            Permissions = permissions.Distinct().Select(p => new RolePermissionEntity { Permission = p }).ToList(),
        };

        await unitOfWork.RoleRepository.AddAsync(entity);
        await unitOfWork.SaveAsync();
    }

    public async Task UpdateRoleAsync(RoleModel role, IReadOnlyCollection<string> permissions)
    {
        var entity = await GetExistingRoleAsync(role.Id);
        var name = await ValidateAsync(role, permissions);

        if (DefaultRoles.Names.ContainsKey(entity.Id) && !string.Equals(entity.Name, name, StringComparison.Ordinal))
        {
            throw new BadRequestException("Default roles cannot be renamed.");
        }

        entity.Name = name;

        var newPermissions = permissions.Distinct().ToList();

        foreach (var removed in entity.Permissions.Where(p => !newPermissions.Contains(p.Permission)).ToList())
        {
            entity.Permissions.Remove(removed);
        }

        foreach (var permission in newPermissions.Where(p => entity.Permissions.All(e => e.Permission != p)))
        {
            entity.Permissions.Add(new RolePermissionEntity { RoleId = entity.Id, Permission = permission });
        }

        await unitOfWork.SaveAsync();
    }

    public async Task DeleteRoleAsync(Guid id)
    {
        var entity = await GetExistingRoleAsync(id);

        if (DefaultRoles.Names.ContainsKey(id))
        {
            throw new BadRequestException("Default roles cannot be deleted.");
        }

        await unitOfWork.RoleRepository.DeleteAsync(entity);
        await unitOfWork.SaveAsync();
    }

    private static RoleModel ToModel(RoleEntity role)
    {
        return new RoleModel { Id = role.Id, Name = role.Name };
    }

    private async Task<RoleEntity> GetExistingRoleAsync(Guid id)
    {
        return await unitOfWork.RoleRepository.GetWithPermissionsAsync(id)
            ?? throw new NotFoundException($"Role with id '{id}' does not exist.");
    }

    private async Task<string> ValidateAsync(RoleModel role, IReadOnlyCollection<string> permissions)
    {
        if (string.IsNullOrWhiteSpace(role.Name))
        {
            throw new BadRequestException("Role name is required.");
        }

        var name = role.Name.Trim();
        var existing = await unitOfWork.RoleRepository.FirstOrDefaultAsync(r => r.Name == name);

        if (existing is not null && existing.Id != role.Id)
        {
            throw new BadRequestException($"Role with name '{name}' already exists.");
        }

        var unknown = permissions.FirstOrDefault(p => !Permissions.All.Contains(p));

        return unknown is null
            ? name
            : throw new BadRequestException($"Permission '{unknown}' does not exist.");
    }
}
