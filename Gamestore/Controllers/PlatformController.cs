using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services.Interaces;
using Gamestore.WebAPI.DTOs.GameDTO;
using Gamestore.WebAPI.DTOs.PlatformDTO;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class PlatformController(IPlatformService platformService, IMapper mapper) : ControllerBase
{
    private readonly IPlatformService _platformService = platformService;
    private readonly IMapper _mapper = mapper;

    [HttpPost("platform")]
    public async Task<IActionResult> CreatePlatformAsync([FromBody] CreatePlatformDTO platformType)
    {
        await _platformService.CreatePlatformAsync(platformType.Platform.Type);

        return NoContent();
    }

    [HttpGet("platform/{platformId}")]
    public async Task<IActionResult> GetPlatformByIdAsync([FromRoute] Guid platformId)
    {
        var platformModel = await _platformService.GetPlatformByIdAsync(platformId);
        var platformDTO = _mapper.Map<FullPlatformDTO>(platformModel);

        return Ok(platformDTO);
    }

    [HttpGet("platforms")]
    public async Task<IActionResult> GetAllPlatformsAsync()
    {
        var platformModels = await _platformService.GetAllPlatformsAsync();
        var platformDTOs = _mapper.Map<List<FullPlatformDTO>>(platformModels);

        return Ok(platformDTOs);
    }

    [HttpPut("platform")]
    public async Task<IActionResult> UpdatePlatformAsync([FromBody] UpdatePlatformDTO platformDTO)
    {
        var model = _mapper.Map<PlatformModel>(platformDTO);

        await _platformService.UpdatePlatformAsync(model);
        return NoContent();
    }

    [HttpDelete("platform/{id}")]
    public async Task<IActionResult> DeletePlatformAsync([FromRoute] Guid id)
    {
        await _platformService.DeletePlatformAsync(id);

        return NoContent();
    }

    [HttpGet("games/platform/{platformId}")]
    public async Task<IActionResult> GetGamesByPlatformIdAsync([FromRoute] Guid platformId)
    {
        var gameModels = await _platformService.GetGamesByPlatformIdAsync(platformId);
        var gameDTOs = _mapper.Map<List<GameWithIdDto>>(gameModels);

        return Ok(gameDTOs);
    }
}
