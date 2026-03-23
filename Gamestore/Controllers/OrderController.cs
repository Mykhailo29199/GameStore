using AutoMapper;
using BusinessLogicLayer.Services.Interaces;
using GameStore.Shared.Stubs;
using Gamestore.WebAPI.DTOs.OrderDTO;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class OrderController(IOrderService orderService, IMapper mapper) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;
    private readonly IMapper _mapper = mapper;

    [HttpPost("games/{key}/buy")]
    public async Task<IActionResult> AddGameToCart(string key)
    {
        await _orderService.AddGameToCartAsync(CustomerStub.StubCustomerId, key);
        return Ok();
    }

    [HttpDelete("orders/cart/{key}")]
    public async Task<IActionResult> RemoveGameFromCart(string key)
    {
        await _orderService.RemoveGameFromCartAsync(CustomerStub.StubCustomerId, key);
        return Ok();
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IEnumerable<OrderDTO>>> GetClosedOrders()
    {
        var orders = await _orderService.GetClosedOrdersAsync();
        var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);

        return Ok(orderDtos);
    }

    [HttpGet("orders/{id}")]
    public async Task<ActionResult<OrderDTO>> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        var orderDto = _mapper.Map<OrderDTO>(order);

        return Ok(orderDto);
    }

    [HttpGet("orders/{id}/details")]
    public async Task<ActionResult<IEnumerable<OrderGameDTO>>> GetOrderDetails(Guid id)
    {
        var models = await _orderService.GetOrderDetailsAsync(id);
        var dtos = _mapper.Map<IEnumerable<OrderGameDTO>>(models);

        return Ok(dtos);
    }

    [HttpGet("orders/cart")]
    public async Task<ActionResult<IEnumerable<OrderGameDTO>>> GetCart()
    {
        var cart = await _orderService.GetCartAsync(CustomerStub.StubCustomerId);
        var cartDtos = _mapper.Map<IEnumerable<OrderGameDTO>>(cart);

        return Ok(cartDtos);
    }
}
