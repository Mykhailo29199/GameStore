using AutoMapper;
using BusinessLogicLayer.Services.Interaces;
using GameStore.Shared.Stubs;
using Gamestore.WebAPI.DTOs.OrderDTO;
using Gamestore.WebAPI.DTOs.PaymentDTO.PaymentResponseDTOs;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class OrderController(IOrderService orderService, IMapper mapper) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;
    private readonly IMapper _mapper = mapper;

    //[HttpGet("orders/payment-methods")]
    //public async Task<IActionResult> GetPaymentMethods()
    //{
    //    var methods = await _orderService.GetPaymentMethodsAsync();
    //    var result = _mapper.Map<IEnumerable<PaymentMethodResponseDTO>>(methods);

    //    return Ok(result);
    //}

    //[HttpGet("orders/payment-methods")]
    //public IActionResult GetPaymentMethods()
    //{
    //    var paymentMethods = new List<PaymentMethodResponseDTO>
    //{
    //    new PaymentMethodResponseDTO
    //    {
    //        ImageUrl = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
    //        Title = "Bank",
    //        Description = "Payment via bank account. You will receive a PDF invoice for payment.",
    //    },
    //    new PaymentMethodResponseDTO
    //    {
    //        ImageUrl = "https://green-api.volia.com/media/6407a7b23eb69_ibox.jpg",
    //        Title = "IBox terminal",
    //        Description = "Payment via the IBox self-service terminal.",
    //    },
    //    new PaymentMethodResponseDTO
    //    {
    //        ImageUrl = "https://cdn-icons-png.flaticon.com/512/349/349221.png",
    //        Title = "Visa",
    //        Description = "Payment by Visa bank card.",
    //    },
    //};

    //    return Ok(new { paymentMethods });
    //}

    [HttpGet("orders/payment-methods")]
    public IActionResult GetPaymentMethods()
    {
        var paymentMethods = new List<PaymentMethodResponseDTO>
    {
        new PaymentMethodResponseDTO
        {
            ImageUrl = "https://cdn-icons-png.flaticon.com/512/3135/3135715.png",
            Title = "Bank",
            Description = "Payment via bank account. You will receive a PDF invoice for payment.",
        },
        new PaymentMethodResponseDTO
        {
            ImageUrl = "https://green-api.volia.com/media/6407a7b23eb69_ibox.jpg",
            Title = "IBox terminal",
            Description = "Payment via the IBox self-service terminal.",
        },
        new PaymentMethodResponseDTO
        {
            ImageUrl = "https://cdn-icons-png.flaticon.com/512/349/349221.png",
            Title = "Visa",
            Description = "Payment by Visa bank card.",
        },
    };

        return Ok(new { paymentMethods });
    }

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
