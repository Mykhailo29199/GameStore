using AutoMapper;
using BusinessLogicLayer.Models.OrderModels;
using BusinessLogicLayer.Services.Interaces;
using DataAccessLayer.Entities;
using DataAccessLayer.StoreContext;
using DataAccessLayer.UnitOfWork;
using GameStore.Shared.Enums;
using GameStore.Shared.Exceptions;

namespace BusinessLogicLayer.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork<GameStoreContext> _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork<GameStoreContext> unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    //  Get paid and cancelled orders
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
        var orderGames = order.OrderGames;
        return _mapper.Map<IEnumerable<OrderGameModel>>(orderGames);
    }

    public async Task<IEnumerable<OrderGameModel>> GetCartAsync(Guid userId)
    {
        var order = await _unitOfWork.OrderRepository.GetActiveCartAsync(userId);
        IEnumerable<OrderGameModel> cartGames = order == null
            ? []
            : _mapper.Map<IEnumerable<OrderGameModel>>(order.OrderGames);

        return cartGames;
    }

    // Add game to cart
    public async Task AddGameAsync(Guid userId, string gameKey)
    {
        var game = await _unitOfWork.GameRepository.GetGameByKeyAsync(gameKey) ?? throw new ArgumentException("Game not found");

        if (game.UnitInStock <= 0)
        {
            throw new ArgumentException("Game is out of stock");
        }

        var order = await _unitOfWork.OrderRepository.GetActiveCartAsync(userId);

        if (order == null)
        {
            order = new OrderEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = userId,
                Status = OrderStatus.Open,
                Date = DateTime.UtcNow,
                OrderGames = [],
            };
            await _unitOfWork.OrderRepository.AddAsync(order);
        }

        var orderGame = await _unitOfWork.OrderGameRepository.GetOrderGameAsync(order.Id, game.Id);

        if (orderGame != null)
        {
            if (orderGame.Quantity + 1 > game.UnitInStock)
            {
                throw new NotFoundException("Not enough stock available");
            }

            orderGame.Quantity++;
        }
        else
        {
            orderGame = new OrderGameEntity
            {
                OrderId = order.Id,
                ProductId = game.Id,
                Price = game.Price,
                Quantity = 1,
                Discount = game.Discount,
            };
            await _unitOfWork.OrderGameRepository.AddAsync(orderGame);
        }

        await _unitOfWork.SaveAsync();
    }

    // Remove game from cart
    public async Task RemoveGameAsync(Guid? userId, string gameKey)
    {
        if (userId == null || userId == Guid.Empty)
        {
            throw new ArgumentException("Guest cart not supported in this implementation");
        }

        var game = await _unitOfWork.GameRepository.GetGameByKeyAsync(gameKey) ?? throw new NotFoundException("Game not found");
        var order = await _unitOfWork.OrderRepository.GetActiveCartAsync(userId.Value) ?? throw new NotFoundException("Active cart not found");
        var orderGame = await _unitOfWork.OrderGameRepository.GetOrderGameAsync(order.Id, game.Id) ?? throw new NotFoundException("Game not found in order");

        await UpdateGameQuantityAsync(orderGame, order);
    }

    public async Task CheckoutAsync(Guid orderId)
    {
        var order = await _unitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId) ?? throw new NotFoundException("Order not found");

        if (order.OrderGames.Count == 0)
        {
            throw new ArgumentException("Cannot checkout empty order");
        }

        order.Status = OrderStatus.Checkout;
        await _unitOfWork.SaveAsync();
    }

    public async Task CompletePaymentAsync(Guid orderId)
    {
        var order = await _unitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId) ?? throw new NotFoundException("Order not found");
        order.Status = OrderStatus.Paid;

        foreach (var og in order.OrderGames)
        {
            if (og.Game.UnitInStock < og.Quantity)
            {
                throw new ArgumentException("Not enough stock for game: " + og.Game.Name);
            }

            og.Game.UnitInStock -= og.Quantity;
        }

        await _unitOfWork.SaveAsync();
    }

    public async Task CancelOrderAsync(Guid orderId)
    {
        var order = await _unitOfWork.OrderRepository.GetOrderWithGamesAsync(orderId) ?? throw new NotFoundException("Order not found");
        order.Status = OrderStatus.Cancelled;

        await _unitOfWork.SaveAsync();
    }

    private async Task UpdateGameQuantityAsync(OrderGameEntity orderGame, OrderEntity order)
    {
        if (orderGame.Quantity > 1)
        {
            orderGame.Quantity--;
        }
        else
        {
            await _unitOfWork.OrderGameRepository.DeleteAsync(orderGame);

            // Get the balance of games in the cart
            var remainingGames = await _unitOfWork.OrderGameRepository.GetOrderGamesAsync(order.Id);
            if (remainingGames.Count == 0)
            {
                await _unitOfWork.OrderRepository.DeleteAsync(order);
            }
        }

        await _unitOfWork.SaveAsync();
    }
}