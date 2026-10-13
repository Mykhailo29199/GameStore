using AutoMapper;
using BusinessLogicLayer.Catalog.Interfaces;
using BusinessLogicLayer.Models.OrderModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.Mongo;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Enums;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class OrderService(
    ISqlServerUnitOfWork unitOfWork,
    IMongoDBUnitOfWork mongoUnitOfWork,
    IProductCatalog productCatalog,
    IMapper mapper) : IOrderService
{
    private readonly ISqlServerUnitOfWork _sqlUnitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<OrderModel>> GetClosedOrdersAsync()
    {
        var orders = await _sqlUnitOfWork.OrderRepository.FindAsync(o =>
            o.Status == OrderStatus.Paid || o.Status == OrderStatus.Cancelled);

        return _mapper.Map<IEnumerable<OrderModel>>(orders);
    }

    public async Task<OrderModel?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _sqlUnitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId);

        if (order is not null)
        {
            return _mapper.Map<OrderModel?>(order);
        }

        var northwindOrder = await GetNorthwindOrderAsync(orderId)
            ?? throw new NotFoundException($"Order with id `{orderId}` was not found.");

        return _mapper.Map<OrderModel?>(northwindOrder);
    }

    public async Task<IEnumerable<OrderGameModel>> GetOrderDetailsAsync(Guid orderId)
    {
        var order = await _sqlUnitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId);

        if (order is not null)
        {
            return _mapper.Map<IEnumerable<OrderGameModel>>(order.OrderDetails);
        }

        var northwindOrder = await GetNorthwindOrderAsync(orderId) ?? throw new NotFoundException("Order not found");

        return await GetNorthwindOrderDetailsAsync(orderId, northwindOrder.OrderID);
    }

    public async Task<IEnumerable<OrderGameModel>> GetCartAsync(Guid userId)
    {
        var order = await _sqlUnitOfWork.OrderRepository.GetActiveCartAsync(userId);
        IEnumerable<OrderGameModel> cartGames = order == null
            ? []
            : _mapper.Map<IEnumerable<OrderGameModel>>(order.OrderDetails);

        return cartGames;
    }

    public async Task AddGameToCartAsync(Guid userId, string gameKey)
    {
        var catalogGame = await productCatalog.GetByKeyAsync(gameKey) ?? throw new NotFoundException("Game not found");

        if (catalogGame.UnitsInStock <= 0)
        {
            throw new BadRequestException("Game is out of stock");
        }

        // Buying a Northwind game copies it to the Game Store database, Northwind stays untouched.
        var game = await productCatalog.EnsureInGameStoreAsync(gameKey);

        var order = await _sqlUnitOfWork.OrderRepository.GetActiveCartAsync(userId);

        if (order == null)
        {
            order = new OrderEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = userId,
                Status = OrderStatus.Open,
                OrderDate = DateTime.UtcNow,
                OrderDetails = [],
            };
            await _sqlUnitOfWork.OrderRepository.AddAsync(order);
        }

        await IncreaseGameQuantityAsync(order, game);

        await _sqlUnitOfWork.SaveAsync();
    }

    public async Task RemoveGameFromCartAsync(Guid userId, string gameKey)
    {
        if (userId == Guid.Empty)
        {
            throw new BadRequestException("Guest cart not supported in this implementation");
        }

        var game = await _sqlUnitOfWork.GameRepository.GetGameByKeyAsync(gameKey) ?? throw new NotFoundException("Game not found");
        var order = await _sqlUnitOfWork.OrderRepository.GetActiveCartAsync(userId) ?? throw new NotFoundException("Active cart not found");
        var orderGame = await _sqlUnitOfWork.OrderGameRepository.GetOrderGameAsync(order.Id, game.Id) ?? throw new NotFoundException("Game not found in order");

        await DecreaseGameQuantityAsync(orderGame, order);
    }

    public async Task CheckoutAsync(Guid orderId)
    {
        var order = await _sqlUnitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId) ?? throw new NotFoundException("Order not found");

        if (order.OrderDetails.Count == 0)
        {
            throw new BadRequestException("Cannot checkout empty order");
        }

        order.Status = OrderStatus.Checkout;
        await _sqlUnitOfWork.SaveAsync();
    }

    public async Task CompletePaymentAsync(Guid orderId)
    {
        var order = await _sqlUnitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId) ?? throw new NotFoundException("Order not found");
        order.Status = OrderStatus.Paid;

        foreach (var og in order.OrderDetails)
        {
            if (og.Product.UnitsInStock < og.Quantity)
            {
                throw new BadRequestException("Not enough stock for game: " + og.Product.ProductName);
            }

            await productCatalog.DecreaseUnitsInStockAsync(og.Product, og.Quantity);
        }

        await _sqlUnitOfWork.SaveAsync();
    }

    public async Task CancelOrderAsync(Guid orderId)
    {
        var order = await _sqlUnitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId) ?? throw new NotFoundException("Order not found");
        order.Status = OrderStatus.Cancelled;

        await _sqlUnitOfWork.SaveAsync();
    }

    /// <summary>
    /// Returns orders from both databases. An empty start or end date means the range is unlimited on that side.
    /// </summary>
    public async Task<IEnumerable<OrderModel>> GetOrdersHistoryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var sqlOrders = await _sqlUnitOfWork.OrderRepository.FindAsync(o => o.Status != OrderStatus.Open);

        var filteredSqlOrders = sqlOrders.Where(o =>
            (!startDate.HasValue || o.OrderDate >= startDate.Value) &&
            (!endDate.HasValue || o.OrderDate < endDate.Value));

        var northwindOrders = await mongoUnitOfWork.OrderRepository.GetByDateRangeAsync(startDate, endDate);

        var sqlModels = _mapper.Map<IEnumerable<OrderModel>>(filteredSqlOrders);
        var northwindModels = _mapper.Map<IEnumerable<OrderModel>>(northwindOrders);

        return sqlModels
            .Concat(northwindModels)
            .OrderByDescending(o => o.Date)
            .ToList();
    }

    private async Task<DataAccessLayer.Entities.NorthwindMDBEntities.OrderMongoDBEntity?> GetNorthwindOrderAsync(Guid orderId)
    {
        return MongoIdConverter.TryGetObjectId(orderId, out var objectId)
            ? await mongoUnitOfWork.OrderRepository.GetByIdAsync(objectId)
            : null;
    }

    private async Task<IEnumerable<OrderGameModel>> GetNorthwindOrderDetailsAsync(Guid orderId, int? northwindOrderId)
    {
        if (!northwindOrderId.HasValue)
        {
            return [];
        }

        var details = await mongoUnitOfWork.OrderDetailsRepository.GetByOrderIdAsync(northwindOrderId.Value);
        var products = (await mongoUnitOfWork.ProductRepository.GetAllAsync())
            .Where(p => p.ProductID.HasValue)
            .GroupBy(p => p.ProductID!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        return details.Select(d =>
        {
            var product = d.ProductID.HasValue ? products.GetValueOrDefault(d.ProductID.Value) : null;

            return new OrderGameModel
            {
                OrderId = orderId,
                ProductId = product is null ? Guid.Empty : MongoIdConverter.ToGuid(product.InternalId),
                GameName = product?.ProductName ?? string.Empty,
                Price = d.UnitPrice,
                Quantity = d.Quantity,
                Discount = d.Discount.HasValue ? (int)Math.Round(d.Discount.Value * 100) : null,
            };
        }).ToList();
    }

    private async Task IncreaseGameQuantityAsync(OrderEntity order, ProductEntity game)
    {
        var orderGame = await _sqlUnitOfWork.OrderGameRepository.GetOrderGameAsync(order.Id, game.Id);

        if (orderGame != null)
        {
            if (orderGame.Quantity + 1 > game.UnitsInStock)
            {
                throw new BadRequestException("Not enough stock available");
            }

            orderGame.Quantity++;
        }
        else
        {
            orderGame = new OrderDetailsEntity
            {
                OrderId = order.Id,
                ProductId = game.Id,
                UnitPrice = game.UnitPrice,
                Quantity = 1,
                Discount = game.Discount,
            };
            await _sqlUnitOfWork.OrderGameRepository.AddAsync(orderGame);
        }
    }

    private async Task DecreaseGameQuantityAsync(OrderDetailsEntity orderGame, OrderEntity order)
    {
        if (orderGame.Quantity > 1)
        {
            orderGame.Quantity--;
        }
        else
        {
            await _sqlUnitOfWork.OrderGameRepository.DeleteAsync(orderGame);

            var remainingGames = await _sqlUnitOfWork.OrderGameRepository.GetOrderGamesAsync(order.Id);
            if (remainingGames.Count == 0)
            {
                await _sqlUnitOfWork.OrderRepository.DeleteAsync(order);
            }
        }

        await _sqlUnitOfWork.SaveAsync();
    }
}
