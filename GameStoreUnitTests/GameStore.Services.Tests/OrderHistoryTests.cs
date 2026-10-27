using System.Globalization;
using System.Linq.Expressions;
using AutoMapper;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Enums;
using Moq;
using Xunit;

namespace GameStoreUnitTests.GameStore.Services.Tests;

public class OrderHistoryTests
{
    private static readonly DateTime From = new(2026, 1, 1);
    private static readonly DateTime To = new(2026, 2, 1);

    private readonly Mock<ISqlServerUnitOfWork> _sqlUnitOfWorkMock = new() { DefaultValue = DefaultValue.Mock };
    private readonly Mock<IMongoDBUnitOfWork> _mongoUnitOfWorkMock = new() { DefaultValue = DefaultValue.Mock };
    private readonly OrderService _orderService;

    private Expression<Func<OrderEntity, bool>>? _sqlPredicate;

    public OrderHistoryTests()
    {
        _sqlUnitOfWorkMock
            .Setup(x => x.OrderRepository.FindAsync(It.IsAny<Expression<Func<OrderEntity, bool>>>()))
            .Callback<Expression<Func<OrderEntity, bool>>>(predicate => _sqlPredicate = predicate)
            .ReturnsAsync([]);
        _mongoUnitOfWorkMock
            .Setup(x => x.OrderRepository.GetByDateRangeAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync([]);

        _orderService = new OrderService(
            _sqlUnitOfWorkMock.Object,
            _mongoUnitOfWorkMock.Object,
            Mock.Of<IProductCatalog>(),
            Mock.Of<IMapper>());
    }

    [Theory]
    [InlineData(OrderStatus.Paid, true)]
    [InlineData(OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Open, false)]
    [InlineData(OrderStatus.Checkout, false)]
    public async Task GetOrdersHistoryAsync_ShouldQueryOnlyFinishedOrders(OrderStatus status, bool expected)
    {
        var matches = await GetSqlFilterAsync(null, null);

        Assert.Equal(expected, matches(new OrderEntity { Status = status, OrderDate = From }));
    }

    [Theory]
    [InlineData("2025-12-31T23:59:59", false)]
    [InlineData("2026-01-01T00:00:00", true)]
    [InlineData("2026-01-31T23:59:59", true)]
    [InlineData("2026-02-01T00:00:00", false)]
    public async Task GetOrdersHistoryAsync_ShouldQueryOrdersFromStartUntilEndDate(string orderDate, bool expected)
    {
        var matches = await GetSqlFilterAsync(From, To);

        Assert.Equal(expected, matches(new OrderEntity { Status = OrderStatus.Paid, OrderDate = DateTime.Parse(orderDate, CultureInfo.InvariantCulture) }));
    }

    [Fact]
    public async Task GetOrdersHistoryAsync_ShouldPassDatesToNorthwind()
    {
        await _orderService.GetOrdersHistoryAsync(From, To);

        _mongoUnitOfWorkMock.Verify(x => x.OrderRepository.GetByDateRangeAsync(From, To), Times.Once);
    }

    private async Task<Func<OrderEntity, bool>> GetSqlFilterAsync(DateTime? startDate, DateTime? endDate)
    {
        await _orderService.GetOrdersHistoryAsync(startDate, endDate);

        Assert.NotNull(_sqlPredicate);
        return _sqlPredicate.Compile();
    }
}
