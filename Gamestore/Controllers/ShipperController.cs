using BusinessLogicLayer.Services.Interaces;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class ShipperController(IShipperService shipperService) : ControllerBase
{
    /// <summary>
    /// Shipper model is dynamic, so the response has a free content structure.
    /// </summary>
    [HttpGet("shippers")]
    public async Task<IActionResult> GetShippers()
    {
        var shippers = await shipperService.GetShippersAsync();

        return Ok(shippers);
    }
}
