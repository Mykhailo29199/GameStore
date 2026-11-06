using AutoMapper;
using BusinessLogicLayer.Models.GameModels;
using BusinessLogicLayer.Services.Interaces;
using GameStore.Shared.Authorization;
using Gamestore.WebAPI.DTOs.GameDTO;
using Gamestore.WebAPI.DTOs.GenreDTO;
using Gamestore.WebAPI.DTOs.PlatformDTO;
using Gamestore.WebAPI.DTOs.PublisherDTO;
using Gamestore.WebAPI.Security;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class GameController(IGameService gameService, IMapper mapper) : ControllerBase
{
    private readonly IGameService _gameService = gameService;
    private readonly IMapper _mapper = mapper;

    [HasPermission(Permissions.AddGame)]

    [HttpPost("game")]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameDTO requestPostDTO)
    {
        var gameModel = _mapper.Map<GameModel>(requestPostDTO);
        await _gameService.CreateGameAsync(gameModel);

        return NoContent();
    }

    [HttpGet("game/{key}")]
    public async Task<IActionResult> GetGameByKey([FromRoute] string key)
    {
        var gameModel = await _gameService.GetGameByKeyAsync(key);
        var gameDTO = _mapper.Map<GameWithIdDTO>(gameModel);

        return Ok(gameDTO);
    }

    [HttpGet("game/{id:guid}")]
    public async Task<IActionResult> GetGameById([FromRoute] Guid id)
    {
        var gameModel = await _gameService.GetGameByIdAsync(id);
        var gameDTO = _mapper.Map<GameWithIdDTO>(gameModel);

        return Ok(gameDTO);
    }

    [HasPermission(Permissions.UpdateGame)]

    [HttpPut("game")]
    public async Task<IActionResult> UpdateGame([FromBody] UpdateGameDTO requestPutDTO)
    {
        var gameModel = _mapper.Map<GameModel>(requestPutDTO);
        await _gameService.UpdateGameAsync(gameModel);

        return NoContent();
    }

    [HasPermission(Permissions.DeleteGame)]

    [HttpDelete("game/{key}")]
    public async Task<IActionResult> DeleteGame([FromRoute] string key)
    {
        await _gameService.DeleteGameAsync(key);

        return NoContent();
    }

    [HttpGet("game/{key}/file")]
    public async Task<IActionResult> DownloadGameFile([FromRoute] string key)
    {
        var filePath = await _gameService.GetGameFilePathAsync(key);

        var fileName = Path.GetFileName(filePath);
        var contentType = "application/octet-stream";

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, contentType, fileName);
    }

    [HttpGet("games/all")]
    public async Task<IActionResult> GetAllGames()
    {
        var gameModels = await _gameService.GetAllGamesAsync();
        var gameDTOs = _mapper.Map<IEnumerable<GameWithIdDTO>>(gameModels);

        return Ok(gameDTOs);
    }

    [HttpGet("games")]
    public async Task<IActionResult> GetGames([FromQuery] GameFilterDTO filterDto)
    {
        var filterModel = _mapper.Map<GameFilterModel>(filterDto);

        var resultModel = await _gameService.GetFilteredGamesAsync(filterModel);

        var responseDto = _mapper.Map<FilteredGamesResponseDTO>(resultModel);

        return Ok(responseDto);
    }

    [HttpGet("genres/game/{key}")]
    public async Task<IActionResult> GetGenresByGameKey([FromRoute] string key)
    {
        var genreModels = await _gameService.GetGenresByGameKeyAsync(key);
        var genreDTOs = _mapper.Map<IEnumerable<GenreDTO>>(genreModels);

        return Ok(genreDTOs);
    }

    [HttpGet("platforms/game/{key}")]
    public async Task<IActionResult> GetPlatformsByGameKey([FromRoute] string key)
    {
        var platformModels = await _gameService.GetPlatformsByGameKeyAsync(key);
        var platformDTOs = _mapper.Map<IEnumerable<FullPlatformDTO>>(platformModels);

        return Ok(platformDTOs);
    }

    [HttpGet("publisher/game/{key}")]
    public async Task<IActionResult> GetPublisherByGameKey([FromRoute] string key)
    {
        var publisherModel = await _gameService.GetPublisherByGameKeyAsync(key);
        var publisherDTO = _mapper.Map<PublisherWithIdDTO>(publisherModel);

        return Ok(publisherDTO);
    }

    [HttpGet("games/pagination-options")]
    public IActionResult GetPaginationOptions()
    {
        var options = _gameService.GetPaginationOptions();
        return Ok(options);
    }

    [HttpGet("games/sorting-options")]
    public IActionResult GetSortingOptions()
    {
        var options = _gameService.GetSortingOptions();
        return Ok(options);
    }

    [HttpGet("games/publish-date-options")]
    public IActionResult GetPublishDateOptions()
    {
        var options = _gameService.GetPublichDateOptions();
        return Ok(options);
    }
}