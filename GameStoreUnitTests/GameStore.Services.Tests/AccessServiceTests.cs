using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Security;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using GameStore.Shared.Authorization;
using Moq;
using Xunit;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class AccessServiceTests
{
    private const string GameKey = "game-key";

    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IProductCatalog> _productCatalogMock = new();
    private readonly AccessService _accessService;

    public AccessServiceTests()
    {
        _accessService = new AccessService(_currentUserMock.Object, _productCatalogMock.Object);
    }

    [Theory]
    [InlineData("Games")]
    [InlineData("Genres")]
    [InlineData("Publisher")]
    [InlineData("Login")]
    public async Task CheckAccessAsync_WhenPageIsReadOnly_ShouldAllowGuest(string page)
    {
        Assert.True(await _accessService.CheckAccessAsync(page, null));
    }

    [Theory]
    [InlineData("AddGame")]
    [InlineData("Basket")]
    [InlineData("History")]
    [InlineData("Users")]
    [InlineData("AddRole")]
    [InlineData("BanComment")]
    public async Task CheckAccessAsync_WhenGuestOpensProtectedPage_ShouldDeny(string page)
    {
        Assert.False(await _accessService.CheckAccessAsync(page, null));
    }

    [Theory]
    [InlineData("AddGame", Permissions.AddGame)]
    [InlineData("UpdateOrder", Permissions.UpdateOrder)]
    [InlineData("ShipOrder", Permissions.ShipOrder)]
    [InlineData("Users", Permissions.ManageUsers)]
    [InlineData("Roles", Permissions.ManageRoles)]
    [InlineData("DeleteComment", Permissions.DeleteComment)]
    public async Task CheckAccessAsync_WhenUserHasPermission_ShouldAllow(string page, string permission)
    {
        _currentUserMock.Setup(x => x.HasPermission(permission)).Returns(true);

        Assert.True(await _accessService.CheckAccessAsync(page, null));
    }

    [Fact]
    public async Task CheckAccessAsync_WhenPageIsUnknown_ShouldDeny()
    {
        Assert.False(await _accessService.CheckAccessAsync("UnknownPage", null));
    }

    [Fact]
    public async Task CheckAccessAsync_WhenManagerUpdatesDeletedGame_ShouldDeny()
    {
        SetupDeletedGame();
        _currentUserMock.Setup(x => x.HasPermission(Permissions.UpdateGame)).Returns(true);

        Assert.False(await _accessService.CheckAccessAsync("UpdateGame", GameKey));
    }

    [Fact]
    public async Task CheckAccessAsync_WhenAdministratorUpdatesDeletedGame_ShouldAllow()
    {
        SetupDeletedGame();
        _currentUserMock.Setup(x => x.HasPermission(Permissions.UpdateGame)).Returns(true);
        _currentUserMock.Setup(x => x.HasPermission(Permissions.UpdateDeletedGame)).Returns(true);

        Assert.True(await _accessService.CheckAccessAsync("UpdateGame", GameKey));
    }

    [Fact]
    public async Task CheckAccessAsync_WhenGameIsDeleted_ShouldNotAllowToBuyIt()
    {
        SetupDeletedGame();
        _currentUserMock.Setup(x => x.HasPermission(It.IsAny<string>())).Returns(true);

        Assert.False(await _accessService.CheckAccessAsync("Buy", GameKey));
    }

    [Fact]
    public async Task CheckAccessAsync_WhenGuestOpensDeletedGame_ShouldDeny()
    {
        SetupDeletedGame();

        Assert.False(await _accessService.CheckAccessAsync("Game", GameKey));
    }

    private void SetupDeletedGame()
    {
        _productCatalogMock.Setup(x => x.GetByKeyAsync(GameKey, true))
            .ReturnsAsync(new ProductEntity { Key = GameKey, IsDeleted = true });
    }
}
