//using DataAccessLayer.Entities;
//using DataAccessLayer.Repositories.Interfaces;
//using DataAccessLayer.StoreContext;
//using GameStore.Shared.Enums;
//using Microsoft.EntityFrameworkCore;

//namespace DataAccessLayer.Repositories;

//public class CartRepository : ICartRepository
//{
//    private readonly GameStoreContext _context;

//    public CartRepository(GameStoreContext context)
//    {
//        _context = context;
//    }

//    public async Task<OrderEntity?> GetOpenOrderAsync(Guid customerId)
//    {
//        return await _context.Orders
//            .Include(o => o.OrderGames)
//            .ThenInclude(i => i.Game)
//            .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.Status == OrderStatus.Open);
//    }

//    public async Task AddOrderAsync(OrderEntity order)
//    {
//        await _context.Orders.AddAsync(order);
//    }

//    public async Task UpdateOrderAsync(OrderEntity order)
//    {
//        _context.Orders.Update(order);
//        await _context.SaveChangesAsync();
//    }

//    public async Task DeleteOrderAsync(OrderEntity order)
//    {
//        _context.Orders.Remove(order);
//        await _context.SaveChangesAsync();
//    }

//    public async Task<OrderGameEntity?> GetOrderGameAsync(Guid orderId, Guid gameId)
//    {
//        return await _context.OrderGames
//            .Include(og => og.Game)
//            .FirstOrDefaultAsync(og => og.OrderId == orderId && og.GameId == gameId);
//    }

//    public async Task AddOrderGameAsync(OrderGameEntity orderGame)
//    {
//        await _context.OrderGames.AddAsync(orderGame);
//    }

//    public async Task UpdateOrderGameAsync(OrderGameEntity orderGame)
//    {
//        _context.OrderGames.Update(orderGame);
//        await _context.SaveChangesAsync();
//    }

//    public async Task DeleteOrderGameAsync(OrderGameEntity orderGame)
//    {
//        _context.OrderGames.Remove(orderGame);
//        await _context.SaveChangesAsync();
//    }

//    public async Task SaveChangesAsync()
//    {
//        await _context.SaveChangesAsync();
//    }
//}
