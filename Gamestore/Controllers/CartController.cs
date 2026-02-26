//using BusinessLogicLayer.Services.Interaces;
//using Gamestore.WebAPI.DTOs.CartDTO;
//using Microsoft.AspNetCore.Mvc;

//namespace Gamestore.WebAPI.Controllers;

//[ApiController]
//[Route("orders")]
//public class CartController : ControllerBase
//{
//    private readonly IOrderService _cartService;

//    public CartController(IOrderService cartService)
//    {
//        _cartService = cartService;
//    }

//    [HttpGet("cart")]
//    public async Task<ActionResult<IEnumerable<OrderDetailsDTO>>> GetCart(Guid customerId)
//    {
//        var cart = await _cartService.GetCartAsync(customerId);
//        return Ok(cart);
//    }

//    [HttpPost("cart/add")]
//    public async Task<IActionResult> AddGame(Guid customerId, Guid gameId, double price, int discount = 0)
//    {
//        await _cartService.AddGameAsync(customerId, gameId, price, discount);
//        return Ok();
//    }

//    [HttpDelete("cart/remove")]
//    public async Task<IActionResult> RemoveGame(Guid customerId, Guid gameId)
//    {
//        await _cartService.RemoveGameAsync(customerId, gameId);
//        return Ok();
//    }
//}


//public class CartController : ControllerBase
//{
//    private readonly IOrderService _cartService;

//    public CartController(IOrderService cartService)
//    {
//        _cartService = cartService;
//    }

//    // Додати гру в кошик
//    [HttpPost("/games/{key}/buy")]
//    public IActionResult AddToCart(string key)
//    {
//        _cartService.AddGame(key);
//        return Ok();
//    }

//    // Видалити гру з кошика
//    [HttpDelete("/orders/cart/{key}")]
//    public IActionResult RemoveFromCart(string key)
//    {
//        _cartService.RemoveGame(key);
//        return Ok();
//    }
//}
