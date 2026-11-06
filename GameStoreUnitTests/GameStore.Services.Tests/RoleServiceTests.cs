using System.Linq.Expressions;
using BusinessLogicLayer.Models.UserModels;
using BusinessLogicLayer.Security;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Authorization;
using GameStore.Shared.Exceptions;
using Moq;
using Xunit;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class RoleServiceTests
{
    private static readonly Guid CustomRoleId = Guid.Parse("c0000000-0000-0000-0000-000000000001");

    private readonly Mock<ISqlServerUnitOfWork> _unitOfWorkMock = new() { DefaultValue = DefaultValue.Mock };
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        _unitOfWorkMock.Setup(x => x.RoleRepository.FirstOrDefaultAsync(It.IsAny<Expression<Func<RoleEntity, bool>>>()))
            .ReturnsAsync((RoleEntity?)null);

        _roleService = new RoleService(_unitOfWorkMock.Object);
    }

    [Fact]
    public void GetPermissions_ShouldReturnAllPermissions()
    {
        Assert.Equal(Permissions.All, _roleService.GetPermissions());
    }

    [Fact]
    public async Task AddRoleAsync_ValidData_ShouldAddRoleWithPermissions()
    {
        // Act
        await _roleService.AddRoleAsync(
            new RoleModel { Name = "test role" },
            [Permissions.AddGame, Permissions.UpdateGame]);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.RoleRepository.AddAsync(It.Is<RoleEntity>(r =>
                r.Name == "test role" &&
                r.Permissions.Select(p => p.Permission).SequenceEqual(new[] { Permissions.AddGame, Permissions.UpdateGame }))),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task AddRoleAsync_WhenPermissionIsUnknown_ShouldThrowBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _roleService.AddRoleAsync(new RoleModel { Name = "test role" }, ["FlyToTheMoon"]));
    }

    [Fact]
    public async Task UpdateRoleAsync_ValidData_ShouldReplacePermissions()
    {
        // Arrange
        var role = new RoleEntity
        {
            Id = CustomRoleId,
            Name = "custom",
            Permissions = [new RolePermissionEntity { RoleId = CustomRoleId, Permission = Permissions.AddGame }],
        };

        _unitOfWorkMock.Setup(x => x.RoleRepository.GetWithPermissionsAsync(CustomRoleId)).ReturnsAsync(role);

        // Act
        await _roleService.UpdateRoleAsync(new RoleModel { Id = CustomRoleId, Name = "renamed" }, [Permissions.BuyGame]);

        // Assert
        Assert.Equal("renamed", role.Name);
        Assert.Equal(Permissions.BuyGame, role.Permissions.Single().Permission);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_WhenRoleIsDefault_ShouldThrowBadRequest()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.RoleRepository.GetWithPermissionsAsync(DefaultRoles.AdminId))
            .ReturnsAsync(new RoleEntity { Id = DefaultRoles.AdminId, Name = DefaultRoles.Admin });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _roleService.DeleteRoleAsync(DefaultRoles.AdminId));
    }

    [Fact]
    public async Task DeleteRoleAsync_WhenRoleDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.RoleRepository.GetWithPermissionsAsync(CustomRoleId))
            .ReturnsAsync((RoleEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _roleService.DeleteRoleAsync(CustomRoleId));
    }

    [Fact]
    public void DefaultRoles_ShouldInheritPermissionsOfLowerRoles()
    {
        var admin = DefaultRoles.PermissionsByRole[DefaultRoles.AdminId];
        var manager = DefaultRoles.PermissionsByRole[DefaultRoles.ManagerId];
        var moderator = DefaultRoles.PermissionsByRole[DefaultRoles.ModeratorId];
        var user = DefaultRoles.PermissionsByRole[DefaultRoles.UserId];
        var guest = DefaultRoles.PermissionsByRole[DefaultRoles.GuestId];

        Assert.Empty(guest);
        Assert.Subset(moderator.ToHashSet(), user.ToHashSet());
        Assert.Subset(manager.ToHashSet(), moderator.ToHashSet());
        Assert.Subset(admin.ToHashSet(), manager.ToHashSet());
        Assert.Equal(Permissions.All.ToHashSet(), admin.ToHashSet());
        Assert.DoesNotContain(Permissions.UpdateDeletedGame, manager);
    }

    [Fact]
    public void PasswordHasher_ShouldVerifyOnlyTheOriginalPassword()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("secret");

        Assert.True(hasher.Verify("secret", hash));
        Assert.False(hasher.Verify("other", hash));
        Assert.False(hasher.Verify("secret", null));
    }
}
