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
    public async Task<IActionResult> CreateGame([FromBody] GameCreateRequestDTO requestPostDTO)
    {
        var gameModel = _mapper.Map<GameModel>(requestPostDTO);
        await _gameService.CreateGameAsync(gameModel);

        return NoContent();
    }
}