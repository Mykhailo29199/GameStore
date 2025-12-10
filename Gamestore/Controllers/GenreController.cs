using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services.Interaces;
using Gamestore.WebAPI.DTOs.GenreDTO;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class GenreController(IGenreService genreService, IMapper mapper) : ControllerBase
{
    private readonly IGenreService _genreService = genreService;
    private readonly IMapper _mapper = mapper;

    [HttpPost]
    public async Task<IActionResult> CreateGenre([FromBody] GenreNameAndParentGenreIdDTO genreDTO)
    {
        var genreModel = _mapper.Map<GenreModel>(genreDTO);
        await _genreService.CreateGenreAsync(genreModel);

        return NoContent();
    }

    [HttpGet("{genreId}")]
    public async Task<IActionResult> GetGenreById([FromRoute] Guid genreId)
    {
        var genreModel = await _genreService.GetGenreByIdAsync(genreId);
        var genreDTO = _mapper.Map<FullGenreDTO>(genreModel);

        return Ok(genreDTO);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGenres()
    {
        var genreModels = await _genreService.GetAllGenresAsync();
        var genreDTOs = _mapper.Map<List<FullGenreDTO>>(genreModels);

        return Ok(genreDTOs);
    }

    [HttpGet("{parentGenreId}/genres")]
    public async Task<IActionResult> GetGenresByParentGenreId([FromRoute] Guid parentGenreId)
    {
        var genreModels = await _genreService.GetGenresByParentGenreIdAsync(parentGenreId);
        var genreDTOs = _mapper.Map<List<GenreDTO>>(genreModels);

        return Ok(genreDTOs);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateGenre([FromBody] FullGenreDTO genreDTO)
    {
        var genreModel = _mapper.Map<GenreModel>(genreDTO);
        await _genreService.UpdateGenreAsync(genreModel);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGenre([FromRoute] Guid id)
    {
        await _genreService.DeleteGenreAsync(id);

        return NoContent();
    }
}
