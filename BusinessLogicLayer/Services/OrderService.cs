using AutoMapper;
using BusinessLogicLayer.Models.OrderModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities.GameStoreSQLEntities;
using DataAccessLayer.UnitOfWork.UnitOfWorkInterfaces;
using GameStore.Shared.Enums;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class OrderService : IOrderService
{
    private readonly ISqlServerUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(ISqlServerUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderModel>> GetClosedOrdersAsync()
    {
        var orders = await _unitOfWork.OrderRepository.FindAsync(o =>
            o.Status == OrderStatus.Paid || o.Status == OrderStatus.Cancelled);

        return _mapper.Map<IEnumerable<OrderModel>>(orders);
    }

    public async Task<OrderModel?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _unitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId)
            ?? throw new NotFoundException($"Order with id `{orderId}` was not found.");

        return _mapper.Map<OrderModel?>(order);
    }

    public async Task<IEnumerable<OrderGameModel>> GetOrderDetailsAsync(Guid orderId)
    {
        var order = await _unitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId) ?? throw new NotFoundException("Order not found");
        var orderGames = order.OrderDetails;
        return _mapper.Map<IEnumerable<OrderGameModel>>(orderGames);
    }

    public async Task<IEnumerable<OrderGameModel>> GetCartAsync(Guid userId)
    {
        var order = await _unitOfWork.OrderRepository.GetActiveCartAsync(userId);
        IEnumerable<OrderGameModel> cartGames = order == null
            ? []
            : _mapper.Map<IEnumerable<OrderGameModel>>(order.OrderDetails);

        return cartGames;
    }

    public async Task AddGameToCartAsync(Guid userId, string gameKey)
    {
        var game = await _unitOfWork.GameRepository.GetGameByKeyAsync(gameKey) ?? throw new NotFoundException("Game not found");

        if (game.UnitsInStock <= 0)
        {
            throw new BadRequestException("Game is out of stock");
        }

        var order = await _unitOfWork.OrderRepository.GetActiveCartAsync(userId);

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
            await _unitOfWork.OrderRepository.AddAsync(order);
        }

        await IncreaseGameQuantityAsync(order, game);

        await _unitOfWork.SaveAsync();
    }

    public async Task RemoveGameFromCartAsync(Guid userId, string gameKey)
    {
        if (userId == Guid.Empty)
        {
            throw new BadRequestException("Guest cart not supported in this implementation");
        }

        var game = await _unitOfWork.GameRepository.GetGameByKeyAsync(gameKey) ?? throw new NotFoundException("Game not found");
        var order = await _unitOfWork.OrderRepository.GetActiveCartAsync(userId) ?? throw new NotFoundException("Active cart not found");
        var orderGame = await _unitOfWork.OrderGameRepository.GetOrderGameAsync(order.Id, game.Id) ?? throw new NotFoundException("Game not found in order");

        await DecreaseGameQuantityAsync(orderGame, order);
    }

    public async Task CheckoutAsync(Guid orderId)
    {
        var order = await _unitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId) ?? throw new NotFoundException("Order not found");

        if (order.OrderDetails.Count == 0)
        {
            throw new BadRequestException("Cannot checkout empty order");
        }

        order.Status = OrderStatus.Checkout;
        await _unitOfWork.SaveAsync();
    }

    public async Task CompletePaymentAsync(Guid orderId)
    {
        var order = await _unitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId) ?? throw new NotFoundException("Order not found");
        order.Status = OrderStatus.Paid;

        foreach (var og in order.OrderDetails)
        {
            if (og.Product.UnitsInStock < og.Quantity)
            {
                throw new BadRequestException("Not enough stock for game: " + og.Product.ProductName);
            }

            og.Product.UnitsInStock -= og.Quantity;
        }

        await _unitOfWork.SaveAsync();
    }

    public async Task CancelOrderAsync(Guid orderId)
    {
        var order = await _unitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId) ?? throw new NotFoundException("Order not found");
        order.Status = OrderStatus.Cancelled;

        await _unitOfWork.SaveAsync();
    }

    private async Task IncreaseGameQuantityAsync(OrderEntity order, ProductEntity game)
    {
        var orderGame = await _unitOfWork.OrderGameRepository.GetOrderGameAsync(order.Id, game.Id);

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
            await _unitOfWork.OrderGameRepository.AddAsync(orderGame);
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
            await _unitOfWork.OrderGameRepository.DeleteAsync(orderGame);

            var remainingGames = await _unitOfWork.OrderGameRepository.GetOrderGamesAsync(order.Id);
            if (remainingGames.Count == 0)
            {
                await _unitOfWork.OrderRepository.DeleteAsync(order);
            }
        }

        await _unitOfWork.SaveAsync();
    }
}