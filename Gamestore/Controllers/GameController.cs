using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services.Interaces;
using Gamestore.WebAPI.DTOs.GameDTO;
using Gamestore.WebAPI.DTOs.GenreDTO;
using Gamestore.WebAPI.DTOs.PlatformDTO;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController(IGameService gameService, IMapper mapper) : ControllerBase
{
    private readonly IGameService _gameService = gameService;
    private readonly IMapper _mapper = mapper;

    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] FullGameDTO requestPostDTO)
    {
        var gameModel = _mapper.Map<FullGameModel>(requestPostDTO);
        await _gameService.CreateGameAsync(gameModel);

        return NoContent();
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetGameByKey([FromRoute] string key)
    {
        var gameModel = await _gameService.GetGameByKeyAsync(key);
        var gameDTO = _mapper.Map<GameWithIdDTO>(gameModel);

        return Ok(gameDTO);
    }

    [HttpGet("find/{gameId}")]
    public async Task<IActionResult> GetGameById([FromRoute] Guid gameId)
    {
        var gameModel = await _gameService.GetGameByIdAsync(gameId);
        var gameDTO = _mapper.Map<GameWithIdDTO>(gameModel);

        return Ok(gameDTO);
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

    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteGame([FromRoute] string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return BadRequest("Invalid request body.");
        }

        await _gameService.DeleteGameAsync(key);

        return NoContent();
    }

    [HttpGet("{gameKey}/file")]
    public async Task<IActionResult> DownloadGameFile([FromRoute] string gameKey)
    {
        var filePath = await _gameService.GetGameFilePathAsync(gameKey);

        if (string.IsNullOrEmpty(filePath))
        {
            return NotFound("Game file not found.");
        }

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"Physical file with path '{filePath}' not found on server.");
        }

        var fileName = Path.GetFileName(filePath);
        var contentType = "application/octet-stream";

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, contentType, fileName);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGames()
    {
        var gameModels = await _gameService.GetAllGamesAsync();
        var gameDTOs = _mapper.Map<IEnumerable<GameWithIdDTO>>(gameModels);

        return Ok(gameDTOs);
    }

    [HttpGet("{key}/genres")]
    public async Task<IActionResult> GetGenresByGameKey([FromRoute] string key)
    {
        var genreModels = await _gameService.GetGenresByGameKeyAsync(key);
        var genreDTOs = _mapper.Map<IEnumerable<GenreDTO>>(genreModels);

        return Ok(genreDTOs);
    }

    [HttpGet("{key}/platforms")]
    public async Task<IActionResult> GetPlatformsByGameKey([FromRoute] string key)
    {
        var platformModels = await _gameService.GetPlatformsByGameKeyAsync(key);
        var platformDTOs = _mapper.Map<IEnumerable<PlatformDTO>>(platformModels);

        return Ok(platformDTOs);
    }
}