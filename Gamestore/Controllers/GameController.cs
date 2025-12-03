using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services.Interaces;
using Gamestore.WebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController(IGameService gameService, IMapper mapper) : ControllerBase
{
    private readonly IGameService _gameService = gameService;
    private readonly IMapper _mapper = mapper;

    [HttpPost("games")]
    public async Task<IActionResult> CreateGame([FromBody] FullGameDTO requestPostDTO)
    {
        var gameModel = _mapper.Map<FullGameModel>(requestPostDTO);
        await _gameService.CreateGameAsync(gameModel);

        return NoContent();
    }

    [HttpGet("games/key/{key}")]
    public async Task<IActionResult> GetGameByKey([FromRoute] string key)
    {
        var gameModel = await _gameService.GetGameByKeyAsync(key);
        var gameDTO = _mapper.Map<GameWithIdDTO>(gameModel);

        return Ok(gameDTO);
    }

    [HttpGet("games/id/{gameId}")]
    public async Task<IActionResult> GetGameById([FromRoute] Guid gameId)
    {
        var gameModel = await _gameService.GetGameByIdAsync(gameId);
        var gameDTO = _mapper.Map<GameWithIdDTO>(gameModel);

        return Ok(gameDTO);
    }

    [HttpGet("platforms/{platformId}/games")]
    public async Task<IActionResult> GetGamesByPlatformId([FromRoute] Guid platformId)
    {
        var gameModels = await _gameService.GetGamesByPlatformIdAsync(platformId);
        var gameDTOs = _mapper.Map<IEnumerable<GameWithIdDTO>>(gameModels);

        return Ok(gameDTOs);
    }

    [HttpGet("genres/{genresId}/games")]
    public async Task<IActionResult> GetGamesByGenreId([FromRoute] Guid genresId)
    {
        var gameModels = await _gameService.GetGamesByGenreIdAsync(genresId);
        var gameDTOs = _mapper.Map<IEnumerable<GameWithIdDTO>>(gameModels);

        return Ok(gameDTOs);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateGame([FromBody] FullGameWithIdDTO requestPutDTO)
    {
        if (requestPutDTO == null || requestPutDTO.GameWithIdDTO == null)
        {
            return BadRequest("Invalid request body.");
        }

        var gameModel = _mapper.Map<FullGameModel>(requestPutDTO);
        await _gameService.UpdateGameAsync(gameModel);

        return NoContent();
    }
}