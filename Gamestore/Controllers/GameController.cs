using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services;
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
        if (requestPostDTO == null || requestPostDTO.Game == null)
        {
            return BadRequest("Invalid request body.");
        }

        var gameModel = _mapper.Map<GameModel>(requestPostDTO);
        await _gameService.CreateGameAsync(gameModel);

        return NoContent();
    }
}