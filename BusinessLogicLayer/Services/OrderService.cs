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

    // Orders older than this period belong to the orders history and cannot be edited.
    private static readonly TimeSpan HistoryAge = TimeSpan.FromDays(30);

    public async Task<IEnumerable<OrderModel>> GetClosedOrdersAsync()
    {
        var historyBoundary = DateTime.UtcNow - HistoryAge;

        var orders = await _sqlUnitOfWork.OrderRepository.FindAsync(o =>
            (o.Status == OrderStatus.Paid || o.Status == OrderStatus.Cancelled || o.Status == OrderStatus.Shipped) &&
            (o.OrderDate == null || o.OrderDate >= historyBoundary));

        return _mapper.Map<IEnumerable<OrderModel>>(orders);
    }

    public async Task<OrderModel?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _sqlUnitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId);

        var model = order is not null
            ? _mapper.Map<OrderModel?>(order)
            : _mapper.Map<OrderModel?>(await GetNorthwindOrderAsync(orderId)
                ?? throw new NotFoundException($"Order with id `{orderId}` was not found."));

        if (model is not null)
        {
            var details = await GetOrderDetailsAsync(orderId);
            model.Sum = OrderTotalCalculator.Total(
                details.Select(d => (d.Price, d.Quantity, (double?)d.Discount)));
        }

        return model;
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

        var game = await _sqlUnitOfWork.ProductRepository.GetGameByKeyAsync(gameKey) ?? throw new NotFoundException("Game not found");
        var order = await _sqlUnitOfWork.OrderRepository.GetActiveCartAsync(userId) ?? throw new NotFoundException("Active cart not found");
        var orderGame = await _sqlUnitOfWork.OrderDetailsRepository.GetOrderGameAsync(order.Id, game.Id) ?? throw new NotFoundException("Game not found in order");

        await DecreaseGameQuantityAsync(orderGame, order);
    }

    public async Task RemoveGamePositionFromCartAsync(Guid userId, string gameKey)
    {
        // A game deleted after it was added to the cart must still be removable from it.
        var game = await productCatalog.GetByKeyAsync(gameKey, includeDeleted: true) ?? throw new NotFoundException("Game not found");
        var order = await _sqlUnitOfWork.OrderRepository.GetActiveCartAsync(userId) ?? throw new NotFoundException("Active cart not found");
        var position = order.OrderDetails.FirstOrDefault(d => d.ProductId == game.Id) ?? throw new NotFoundException("Game not found in order");

        if (order.OrderDetails.Count == 1)
        {
            // The cart becomes empty, so the open order is removed together with its last position.
            await _sqlUnitOfWork.OrderRepository.DeleteAsync(order);
        }
        else
        {
            await _sqlUnitOfWork.OrderDetailsRepository.DeleteAsync(position);
        }

        await _sqlUnitOfWork.SaveAsync();
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

    public async Task CancelOrderAsync(Guid orderId)
    {
        var order = await _sqlUnitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId) ?? throw new NotFoundException("Order not found");
        order.Status = OrderStatus.Cancelled;

        await _sqlUnitOfWork.SaveAsync();
    }

    /// <summary>
    /// Returns orders from both databases. Without a date range only orders older than 30 days are returned;
    /// when a range is given, an empty start or end date means it is unlimited on that side.
    /// </summary>
    public async Task<IEnumerable<OrderModel>> GetOrdersHistoryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (!startDate.HasValue && !endDate.HasValue)
        {
            endDate = DateTime.UtcNow - HistoryAge;
        }

        // Open carts and orders still waiting for payment are not part of the history.
        var sqlOrders = await _sqlUnitOfWork.OrderRepository.FindAsync(o =>
            o.Status != OrderStatus.Open &&
            o.Status != OrderStatus.Checkout &&
            (!startDate.HasValue || o.OrderDate >= startDate.Value) &&
            (!endDate.HasValue || o.OrderDate < endDate.Value));

        var northwindOrders = await mongoUnitOfWork.OrderRepository.GetByDateRangeAsync(startDate, endDate);

        var sqlModels = _mapper.Map<IEnumerable<OrderModel>>(sqlOrders);
        var northwindModels = _mapper.Map<IEnumerable<OrderModel>>(northwindOrders);

        return sqlModels
            .Concat(northwindModels)
            .OrderByDescending(o => o.Date)
            .ToList();
    }

    public async Task UpdateOrderDetailQuantityAsync(Guid orderDetailId, int count)
    {
        if (count < 1)
        {
            throw new BadRequestException("Quantity must be greater than zero.");
        }

        var orderDetail = await GetEditableOrderDetailAsync(orderDetailId);

        if (orderDetail.Product is not null && count > orderDetail.Product.UnitsInStock)
        {
            throw new BadRequestException("Not enough stock available");
        }

        orderDetail.Quantity = count;

        await _sqlUnitOfWork.SaveAsync();
    }

    public async Task DeleteOrderDetailAsync(Guid orderDetailId)
    {
        var orderDetail = await GetEditableOrderDetailAsync(orderDetailId);

        await _sqlUnitOfWork.OrderDetailsRepository.DeleteAsync(orderDetail);
        await _sqlUnitOfWork.SaveAsync();
    }

    /// <summary>
    /// Only a paid order can be shipped.
    /// </summary>
    public async Task ShipOrderAsync(Guid orderId)
    {
        var order = await _sqlUnitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId)
            ?? throw new NotFoundException($"Order with id `{orderId}` was not found.");

        if (order.Status != OrderStatus.Paid)
        {
            throw new BadRequestException("Only a paid order can be shipped.");
        }

        order.Status = OrderStatus.Shipped;
        order.ShippedDate = DateTime.UtcNow;

        await _sqlUnitOfWork.SaveAsync();
    }

    public async Task AddGameToOrderAsync(Guid orderId, string gameKey)
    {
        var order = await _sqlUnitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId)
            ?? throw new NotFoundException($"Order with id `{orderId}` was not found.");

        EnsureOrderIsEditable(order);

        var catalogGame = await productCatalog.GetByKeyAsync(gameKey) ?? throw new NotFoundException("Game not found");

        if (catalogGame.UnitsInStock <= 0)
        {
            throw new BadRequestException("Game is out of stock");
        }

        var game = await productCatalog.EnsureInGameStoreAsync(gameKey);

        await IncreaseGameQuantityAsync(order, game);
        await _sqlUnitOfWork.SaveAsync();
    }

    /// <summary>
    /// Shipped and cancelled orders, and orders from the history (older than 30 days), cannot be edited.
    /// Northwind orders are read-only, so they are never found here.
    /// </summary>
    private static void EnsureOrderIsEditable(OrderEntity order)
    {
        if (order.Status is OrderStatus.Shipped or OrderStatus.Cancelled)
        {
            throw new BadRequestException("Shipped and cancelled orders cannot be edited.");
        }

        if (order.OrderDate.HasValue && order.OrderDate.Value < DateTime.UtcNow - HistoryAge)
        {
            throw new BadRequestException("Orders from the history cannot be edited.");
        }
    }

    private async Task<OrderDetailsEntity> GetEditableOrderDetailAsync(Guid orderDetailId)
    {
        var orderDetail = await _sqlUnitOfWork.OrderDetailsRepository.GetDetailWithOrderAsync(orderDetailId)
            ?? throw new NotFoundException($"Order detail with id `{orderDetailId}` was not found.");

        if (orderDetail.Order is not null)
        {
            EnsureOrderIsEditable(orderDetail.Order);
        }

        return orderDetail;
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
        var productIds = details.Select(d => d.ProductID).Where(id => id.HasValue).Distinct().ToList();
        var products = (await mongoUnitOfWork.ProductRepository.FindAsync(p => productIds.Contains(p.ProductID)))
            .Where(p => p.ProductID.HasValue)
            .GroupBy(p => p.ProductID!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        return details.Select(d =>
        {
            var product = d.ProductID.HasValue ? products.GetValueOrDefault(d.ProductID.Value) : null;

            return new OrderGameModel
            {
                Id = MongoIdConverter.ToGuid(d.InternalId),
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
        var orderGame = await _sqlUnitOfWork.OrderDetailsRepository.GetOrderGameAsync(order.Id, game.Id);

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
            await _sqlUnitOfWork.OrderDetailsRepository.AddAsync(orderGame);
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
            await _sqlUnitOfWork.OrderDetailsRepository.DeleteAsync(orderGame);

            var remainingGames = await _sqlUnitOfWork.OrderDetailsRepository.GetOrderGamesAsync(order.Id);
            if (remainingGames.Count == 0)
            {
                await _sqlUnitOfWork.OrderRepository.DeleteAsync(order);
            }
        }

        await _sqlUnitOfWork.SaveAsync();
    }
}
