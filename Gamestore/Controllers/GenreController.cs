using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services.Interaces;
using Gamestore.WebAPI.DTOs.GameDTO;
using Gamestore.WebAPI.DTOs.GenreDTO;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class GenreController(IGenreService genreService, IMapper mapper) : ControllerBase
{
    private readonly IGenreService _genreService = genreService;
    private readonly IMapper _mapper = mapper;

    [HttpPost("genre")]
    public async Task<IActionResult> CreateGenreAsync([FromBody] CreateGenreDTO genreDTO)
    {
        var genreModel = _mapper.Map<GenreModel>(genreDTO);
        await _genreService.CreateGenreAsync(genreModel);

        return NoContent();
    }

    [HttpGet("genre/{genreId}")]
    public async Task<IActionResult> GetGenreByIdAsync([FromRoute] Guid genreId)
    {
        var genreModel = await _genreService.GetGenreByIdAsync(genreId);
        var genreDTO = _mapper.Map<FullGenreDTO>(genreModel);

        return Ok(genreDTO);
    }

    [HttpGet("genres")]
    public async Task<IActionResult> GetAllGenresAsync()
    {
        var genreModels = await _genreService.GetAllGenresAsync();
        var genreDTOs = _mapper.Map<List<FullGenreDTO>>(genreModels);

        return Ok(genreDTOs);
    }

    [HttpGet("genres/{parentGenreId}")]
    public async Task<IActionResult> GetGenresByParentGenreIdAsync([FromRoute] Guid parentGenreId)
    {
        var genreModels = await _genreService.GetGenresByParentGenreIdAsync(parentGenreId);
        var genreDTOs = _mapper.Map<List<GenreDTO>>(genreModels);

        return Ok(genreDTOs);
    }

    [HttpPut("genre")]
    public async Task<IActionResult> UpdateGenreAsync([FromBody] UpdateGenreDTO genreDTO)
    {
        var genreModel = _mapper.Map<GenreModel>(genreDTO);
        await _genreService.UpdateGenreAsync(genreModel);

        return NoContent();
    }

    [HttpDelete("genre/{id}")]
    public async Task<IActionResult> DeleteGenreAsync([FromRoute] Guid id)
    {
        await _genreService.DeleteGenreAsync(id);

        return NoContent();
    }

    [HttpGet("games/genre/{genreId}")]
    public async Task<IActionResult> GetGamesByGenreIdAsync([FromRoute] Guid genreId)
    {
        var gameModels = await _genreService.GetGamesByGenreIdAsync(genreId);
        var gameDTOs = _mapper.Map<IEnumerable<GameWithIdDto>>(gameModels);

        return Ok(gameDTOs);
    }
}
