using AutoMapper;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Enums;
using GameStore.Shared.Exceptions;
using Moq;
using Xunit;
using static GameStoreUnitTests.GameStore.Services.Tests.TestConstants;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class OrderServiceTests
{
    private static readonly Guid UserId = Guid.Parse("d0000000-0000-0000-0000-000000000001");

    private readonly Mock<ISqlServerUnitOfWork> _unitOfWorkMock = new() { DefaultValue = DefaultValue.Mock };
    private readonly Mock<IProductCatalog> _productCatalogMock = new();
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _orderService = new OrderService(
            _unitOfWorkMock.Object,
            new Mock<IMongoDBUnitOfWork>().Object,
            _productCatalogMock.Object,
            new Mock<IMapper>().Object);
    }

    [Fact]
    public async Task RemoveGamePositionFromCartAsync_WhenCartHasOtherGames_ShouldRemoveOnlyThatPosition()
    {
        // Arrange
        var position = new OrderDetailsEntity { ProductId = ExistingGameId, Quantity = 5 };
        var otherPosition = new OrderDetailsEntity { ProductId = ExistingGameId2, Quantity = 1 };
        var cart = SetupCart(position, otherPosition);

        // Act
        await _orderService.RemoveGamePositionFromCartAsync(UserId, ExistingGameKey);

        // Assert
        _unitOfWorkMock.Verify(x => x.OrderDetailsRepository.DeleteAsync(position), Times.Once);
        _unitOfWorkMock.Verify(x => x.OrderRepository.DeleteAsync(cart), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveGamePositionFromCartAsync_WhenItIsTheLastPosition_ShouldRemoveTheCart()
    {
        // Arrange
        var cart = SetupCart(new OrderDetailsEntity { ProductId = ExistingGameId, Quantity = 3 });

        // Act
        await _orderService.RemoveGamePositionFromCartAsync(UserId, ExistingGameKey);

        // Assert
        _unitOfWorkMock.Verify(x => x.OrderRepository.DeleteAsync(cart), Times.Once);
        _unitOfWorkMock.Verify(x => x.OrderDetailsRepository.DeleteAsync(It.IsAny<OrderDetailsEntity>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveGamePositionFromCartAsync_WhenGameIsNotInCart_ShouldThrowNotFound()
    {
        // Arrange
        SetupCart(new OrderDetailsEntity { ProductId = ExistingGameId2, Quantity = 1 });

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _orderService.RemoveGamePositionFromCartAsync(UserId, ExistingGameKey));
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task RemoveGamePositionFromCartAsync_WhenCartDoesNotExist_ShouldThrowNotFound()
    {
        // Arrange
        _productCatalogMock.Setup(x => x.GetByKeyAsync(ExistingGameKey, true))
            .ReturnsAsync(new ProductEntity { Id = ExistingGameId, Key = ExistingGameKey });
        _unitOfWorkMock.Setup(x => x.OrderRepository.GetActiveCartAsync(UserId))
            .ReturnsAsync((OrderEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _orderService.RemoveGamePositionFromCartAsync(UserId, ExistingGameKey));
    }

    [Fact]
    public async Task RemoveGamePositionFromCartAsync_WhenGameWasDeleted_ShouldStillRemoveIt()
    {
        // Arrange
        var position = new OrderDetailsEntity { ProductId = ExistingGameId, Quantity = 1 };
        var cart = SetupCart(position);

        _productCatalogMock.Setup(x => x.GetByKeyAsync(ExistingGameKey, true))
            .ReturnsAsync(new ProductEntity { Id = ExistingGameId, Key = ExistingGameKey, IsDeleted = true });

        // Act
        await _orderService.RemoveGamePositionFromCartAsync(UserId, ExistingGameKey);

        // Assert
        _unitOfWorkMock.Verify(x => x.OrderRepository.DeleteAsync(cart), Times.Once);
    }

    private OrderEntity SetupCart(params OrderDetailsEntity[] positions)
    {
        var cart = new OrderEntity
        {
            CustomerId = UserId,
            Status = OrderStatus.Open,
            OrderDetails = positions.ToList(),
        };

        _productCatalogMock.Setup(x => x.GetByKeyAsync(ExistingGameKey, true))
            .ReturnsAsync(new ProductEntity { Id = ExistingGameId, Key = ExistingGameKey });
        _unitOfWorkMock.Setup(x => x.OrderRepository.GetActiveCartAsync(UserId)).ReturnsAsync(cart);

        return cart;
    }
}
