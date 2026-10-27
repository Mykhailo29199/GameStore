using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Models.Payment;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Enums;
using GameStore.Shared.Enums.PaymentEnums;
using GameStore.Shared.Exceptions;
using GameStore.Shared.FileSystem;
using GameStore.Shared.Stubs;
using Moq;
using Xunit;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class PaymentServiceTests
{
    private readonly Mock<ISqlServerUnitOfWork> _unitOfWorkMock = new() { DefaultValue = DefaultValue.Mock };
    private readonly Mock<IProductCatalog> _productCatalogMock = new();
    private readonly ProductEntity _game = new() { Id = Guid.NewGuid(), Key = "nw_chai", ProductName = "Chai", UnitsInStock = 3 };
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        _unitOfWorkMock.Setup(x => x.GameRepository.GetByIdAsync(_game.Id)).ReturnsAsync(_game);

        _paymentService = new PaymentService(
            httpClientFactoryMock.Object,
            _unitOfWorkMock.Object,
            new Mock<IFileSystem>().Object,
            _productCatalogMock.Object);
    }

    [Fact]
    public async Task ProcessPayment_WhenStockIsNotEnough_ShouldThrowBeforePayment()
    {
        // Arrange
        var order = SetupCart(quantity: 4);

        // Act
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _paymentService.ProcessPayment(new PaymentRequestModel { Method = PaymentOptions.Bank }));

        // Assert
        Assert.Equal(OrderStatus.Open, order.Status);
        Assert.Equal(3, _game.UnitsInStock);
        _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Never);
        _productCatalogMock.Verify(x => x.SyncUnitsInStockAsync(It.IsAny<IReadOnlyCollection<ProductEntity>>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPayment_WhenPaid_ShouldSyncNorthwindAfterGameStoreIsSaved()
    {
        // Arrange
        var order = SetupCart(quantity: 2);
        var calls = new List<string>();

        _unitOfWorkMock.Setup(x => x.SaveAsync()).Callback(() => calls.Add($"save {order.Status}"));
        _productCatalogMock.Setup(x => x.SyncUnitsInStockAsync(It.IsAny<IReadOnlyCollection<ProductEntity>>()))
            .Callback((IReadOnlyCollection<ProductEntity> products) => calls.Add($"sync {products.Count}"));

        // Act
        await _paymentService.ProcessPayment(new PaymentRequestModel { Method = PaymentOptions.Bank });

        // Assert
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(1, _game.UnitsInStock);
        Assert.Equal(["save Checkout", "sync 0", "save Paid", "sync 1"], calls);
    }

    private OrderEntity SetupCart(int quantity)
    {
        var order = new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = CustomerStub.StubCustomerId,
            Status = OrderStatus.Open,
            OrderDetails = [new OrderDetailsEntity { ProductId = _game.Id, Quantity = quantity, UnitPrice = 10 }],
        };

        _unitOfWorkMock.Setup(x => x.OrderRepository.GetActiveCartAsync(CustomerStub.StubCustomerId)).ReturnsAsync(order);

        return order;
    }
}
