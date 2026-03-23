using AutoMapper;
using BusinessLogicLayer.Models.BanModels;
using BusinessLogicLayer.Services.Interaces;
using Gamestore.WebAPI.DTOs.Users;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class BanController(IBanService banService, IMapper mapper) : ControllerBase
{
    [HttpGet("comments/ban/durations")]
    public IActionResult GetBanDurations()
    {
        var result = banService
        .GetBanDurations();

        return Ok(result);
    }

    [HttpPost("comments/ban")]
    public async Task<IActionResult> BanUser([FromBody] BanRequestDTO dto)
    {
        await banService.BanUserAsync(
            mapper.Map<BanModel>(dto));

        return Ok();
    }
}
