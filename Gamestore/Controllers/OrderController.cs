using AutoMapper;
using BusinessLogicLayer.Security;
using BusinessLogicLayer.Services.Interaces;
using GameStore.Shared.Authorization;
using GameStore.Shared.Exceptions;
using Gamestore.WebAPI.DTOs.OrderDTO;
using Gamestore.WebAPI.Helpers;
using Gamestore.WebAPI.Security;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class OrderController(IOrderService orderService, ICurrentUser currentUser, IMapper mapper) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;
    private readonly IMapper _mapper = mapper;

    private Guid CurrentUserId => currentUser.Id
        ?? throw new UnauthorizedException("Only authorized users can work with the cart.");

    [HasPermission(Permissions.BuyGame)]
    [HttpPost("games/{key}/buy")]
    public async Task<IActionResult> AddGameToCart(string key)
    {
        await _orderService.AddGameToCartAsync(CurrentUserId, key);
        return Ok();
    }

    [HasPermission(Permissions.BuyGame)]
    [HttpDelete("orders/cart/{key}")]
    public async Task<IActionResult> RemoveGameFromCart(string key)
    {
        await _orderService.RemoveGameFromCartAsync(CurrentUserId, key);
        return Ok();
    }

    [HasPermission(Permissions.BuyGame)]
    [HttpDelete("orders/cart/{key}/all")]
    public async Task<IActionResult> RemoveGamePositionFromCart(string key)
    {
        await _orderService.RemoveGamePositionFromCartAsync(CurrentUserId, key);
        return NoContent();
    }

    [HasPermission(Permissions.ViewOrders)]
    [HttpGet("orders")]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetClosedOrders()
    {
        var orders = await _orderService.GetClosedOrdersAsync();
        var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);

        return Ok(orderDtos);
    }

    [HasPermission(Permissions.ViewOrders)]
    [HttpGet("orders/{id:guid}")]
    public async Task<ActionResult<OrderDTO>> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        var orderDto = _mapper.Map<OrderDTO>(order);

        return Ok(orderDto);
    }

    [HasPermission(Permissions.ViewOrders)]
    [HttpGet("orders/{id:guid}/details")]
    public async Task<ActionResult<IEnumerable<OrderGameDTO>>> GetOrderDetails(Guid id)
    {
        var models = await _orderService.GetOrderDetailsAsync(id);
        var dtos = _mapper.Map<IEnumerable<OrderGameDTO>>(models);

        return Ok(dtos);
    }

    [HasPermission(Permissions.BuyGame)]
    [HttpGet("orders/cart")]
    public async Task<ActionResult<IEnumerable<OrderGameDTO>>> GetCart()
    {
        var cart = await _orderService.GetCartAsync(CurrentUserId);
        var cartDtos = _mapper.Map<IEnumerable<OrderGameDTO>>(cart);

        return Ok(cartDtos);
    }

    [HasPermission(Permissions.ViewOrderHistory)]
    [HttpGet("orders/history")]
    [ProducesResponseType(typeof(IEnumerable<OrderDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory([FromQuery] string? start, [FromQuery] string? end)
    {
        var historyModels = await _orderService.GetOrdersHistoryAsync(
            QueryDateParser.ParseStartDate(start),
            QueryDateParser.ParseEndDate(end));
        var result = _mapper.Map<IEnumerable<OrderDTO>>(historyModels);

        return Ok(result);
    }

    [HasPermission(Permissions.UpdateOrder)]
    [HttpPatch("orders/details/{id:guid}/quantity")]
    public async Task<IActionResult> UpdateOrderDetailQuantity(Guid id, [FromBody] UpdateQuantityDTO request)
    {
        await _orderService.UpdateOrderDetailQuantityAsync(id, request.Count);

        return NoContent();
    }

    [HasPermission(Permissions.UpdateOrder)]
    [HttpDelete("orders/details/{id:guid}")]
    public async Task<IActionResult> DeleteOrderDetail(Guid id)
    {
        await _orderService.DeleteOrderDetailAsync(id);

        return NoContent();
    }

    [HasPermission(Permissions.ShipOrder)]
    [HttpPost("orders/{id:guid}/ship")]
    public async Task<IActionResult> ShipOrder(Guid id)
    {
        await _orderService.ShipOrderAsync(id);

        return NoContent();
    }

    [HasPermission(Permissions.UpdateOrder)]
    [HttpPost("orders/{id:guid}/details/{key}")]
    public async Task<IActionResult> AddGameToOrder(Guid id, string key)
    {
        await _orderService.AddGameToOrderAsync(id, key);

        return NoContent();
    }
}
