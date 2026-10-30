using System.Linq.Expressions;
using BusinessLogicLayer.Models.UserModels;
using BusinessLogicLayer.Security;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Authorization;
using GameStore.Shared.Exceptions;
using Moq;
using Xunit;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class UserServiceTests
{
    private const string UserName = "vitalii";
    private const string Password = "SuperSecuredPassword";

    private readonly Mock<ISqlServerUnitOfWork> _unitOfWorkMock = new() { DefaultValue = DefaultValue.Mock };
    private readonly Mock<IExternalAuthClient> _externalAuthClientMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly PasswordHasher _passwordHasher = new();
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByNameWithRolesAsync(It.IsAny<string>()))
            .ReturnsAsync((UserEntity?)null);
        _unitOfWorkMock.Setup(x => x.UserRepository.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserEntity, bool>>>()))
            .ReturnsAsync((UserEntity?)null);
        _unitOfWorkMock.Setup(x => x.RoleRepository.GetPermissionsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync([Permissions.BuyGame, Permissions.AddComment]);
        _unitOfWorkMock.Setup(x => x.RoleRepository.GetAllAsync())
            .ReturnsAsync([new RoleEntity { Id = DefaultRoles.UserId, Name = DefaultRoles.User }]);

        _userService = new UserService(
            _unitOfWorkMock.Object,
            _passwordHasher,
            _externalAuthClientMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WhenInternalCredentialsAreValid_ShouldReturnUserWithPermissions()
    {
        // Arrange
        var user = CreateUser();
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByNameWithRolesAsync(UserName)).ReturnsAsync(user);

        // Act
        var result = await _userService.LoginAsync(new LoginModel { Login = UserName, Password = Password, InternalAuth = true });

        // Assert
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(UserName, result.Name);
        Assert.Equal([Permissions.BuyGame, Permissions.AddComment], result.Permissions);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsWrong_ShouldThrowUnauthorized()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByNameWithRolesAsync(UserName)).ReturnsAsync(CreateUser());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _userService.LoginAsync(new LoginModel { Login = UserName, Password = "wrong", InternalAuth = true }));
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldThrowUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _userService.LoginAsync(new LoginModel { Login = UserName, Password = Password, InternalAuth = true }));
    }

    [Fact]
    public async Task LoginAsync_WhenExternalServiceConfirmsNewUser_ShouldCreateUserWithUserRole()
    {
        // Arrange
        _externalAuthClientMock.Setup(x => x.AuthenticateAsync(UserName, Password)).ReturnsAsync(true);

        // Act
        var result = await _userService.LoginAsync(new LoginModel { Login = UserName, Password = Password, InternalAuth = false });

        // Assert
        Assert.Equal(UserName, result.Name);
        _unitOfWorkMock.Verify(
            x => x.UserRepository.AddAsync(It.Is<UserEntity>(u =>
                u.Name == UserName && u.UserRoles.Single().RoleId == DefaultRoles.UserId)),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenExternalServiceRejectsUser_ShouldThrowUnauthorized()
    {
        // Arrange
        _externalAuthClientMock.Setup(x => x.AuthenticateAsync(UserName, Password)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _userService.LoginAsync(new LoginModel { Login = UserName, Password = Password, InternalAuth = false }));
    }

    [Fact]
    public async Task AddUserAsync_ValidData_ShouldStoreHashedPasswordAndRoles()
    {
        // Act
        await _userService.AddUserAsync(new UserModel { Name = UserName }, [DefaultRoles.UserId], Password);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.UserRepository.AddAsync(It.Is<UserEntity>(u =>
                u.Name == UserName &&
                u.PasswordHash != Password &&
                _passwordHasher.Verify(Password, u.PasswordHash) &&
                u.UserRoles.Single().RoleId == DefaultRoles.UserId)),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ValidData_ShouldCreateUserWithUserRoleOnlyAndLogIn()
    {
        // Arrange
        UserEntity? created = null;

        _unitOfWorkMock.Setup(x => x.UserRepository.AddAsync(It.IsAny<UserEntity>()))
            .Callback<UserEntity>(user => created = user)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.UserRepository.GetByNameWithRolesAsync(UserName))
            .ReturnsAsync(() => created);

        // Act
        var result = await _userService.RegisterAsync(UserName, Password);

        // Assert
        Assert.NotNull(created);
        Assert.Equal(DefaultRoles.UserId, created.UserRoles.Single().RoleId);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(UserName, result.Name);
    }

    [Fact]
    public async Task RegisterAsync_WhenNameIsTaken_ShouldThrowBadRequest()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.UserRepository.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserEntity, bool>>>()))
            .ReturnsAsync(CreateUser());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _userService.RegisterAsync(UserName, Password));
    }

    [Fact]
    public async Task AddUserAsync_WhenNameIsTaken_ShouldThrowBadRequest()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.UserRepository.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserEntity, bool>>>()))
            .ReturnsAsync(CreateUser());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _userService.AddUserAsync(new UserModel { Name = UserName }, [], Password));
    }

    [Fact]
    public async Task AddUserAsync_WhenRoleDoesNotExist_ShouldThrowNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _userService.AddUserAsync(new UserModel { Name = UserName }, [Guid.NewGuid()], Password));
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserDeletesOwnAccount_ShouldThrowBadRequest()
    {
        // Arrange
        var user = CreateUser();
        _unitOfWorkMock.Setup(x => x.UserRepository.GetWithRolesAsync(user.Id)).ReturnsAsync(user);
        _currentUserMock.Setup(x => x.Id).Returns(user.Id);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _userService.DeleteUserAsync(user.Id));
    }

    private UserEntity CreateUser()
    {
        return new UserEntity
        {
            Name = UserName,
            PasswordHash = _passwordHasher.Hash(Password),
            UserRoles = [new UserRoleEntity { RoleId = DefaultRoles.UserId }],
        };
    }
}
