using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services.Interaces;
using Gamestore.WebAPI.DTOs.GameDTO;
using Gamestore.WebAPI.DTOs.PlatformDTO;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PlatformController(IPlatformService platformService, IMapper mapper) : ControllerBase
{
    private readonly IPlatformService _platformService = platformService;
    private readonly IMapper _mapper = mapper;

    [HttpPost]
    public async Task<IActionResult> CreatePlatformAsync([FromBody] PlatformDTO platformType)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _platformService.CreatePlatformAsync(platformType.Type);

        return NoContent();
    }

    [HttpGet("{platformId}")]
    public async Task<IActionResult> GetPlatformByIdAsync([FromRoute] Guid platformId)
    {
        var platformModel = await _platformService.GetPlatformByIdAsync(platformId);
        var platformDTO = _mapper.Map<PlatformDTO>(platformModel);

        return Ok(platformDTO);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPlatformsAsync()
    {
        var platformModels = await _platformService.GetAllPlatformsAsync();
        var platformDTOs = _mapper.Map<List<PlatformDTO>>(platformModels);

        return Ok(platformDTOs);
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePlatformAsync([FromBody] PlatformDTO platformDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var model = _mapper.Map<PlatformModel>(platformDTO);

        await _platformService.UpdatePlatformAsync(model);
        return NoContent();
    }

    [HttpDelete("{platformId}")]
    public async Task<IActionResult> DeletePlatformAsync([FromRoute] Guid platformId)
    {
        await _platformService.DeletePlatformAsync(platformId);

        return NoContent();
    }

    [HttpGet("{platformId}/games")]
    public async Task<IActionResult> GetGamesByPlatformIdAsync([FromRoute] Guid platformId)
    {
        var gameModels = await _platformService.GetGamesByPlatformIdAsync(platformId);
        var gameDTOs = _mapper.Map<List<GameWithIdDTO>>(gameModels);

        return Ok(gameDTOs);
    }
}
