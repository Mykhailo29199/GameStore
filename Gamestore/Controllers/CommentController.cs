using AutoMapper;
using BusinessLogicLayer.Models.CommentModels;
using BusinessLogicLayer.Services.Interaces;
using Gamestore.WebAPI.DTOs.CommentDTOs;
using Gamestore.WebAPI.DTOs.Users;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class CommentController(ICommentService commentService, IMapper mapper) : ControllerBase
{
    // Add Comment
    [HttpPost("games/{key}/comments")]
    public async Task<IActionResult> AddComment(string key, [FromBody] AddCommentRequestDTO dto)
    {
        await commentService.AddCommentAsync(key, mapper.Map<AddCommentModel>(dto));

        return Ok();
    }

    [HttpGet("games/{key}/comments")]
    public async Task<IActionResult> GetComments(string key)
    {
        var result = await commentService.GetGameCommentsAsync(key);

        return Ok(result);
    }

    [HttpDelete("games/{key}/comments/{id}")]
    public async Task<IActionResult> DeleteComment(string key, Guid id)
    {
        await commentService.DeleteCommentAsync(key, id);

        return NoContent();
    }

    [HttpGet("comments/ban/durations")]
    public IActionResult GetBanDurations()
    {
        var result = commentService
        .GetBanDurations()
        .Select(x => x.Duration)
        .ToList();

        return Ok(result);
    }

    [HttpPost("comments/ban")]
    public async Task<IActionResult> BanUser([FromBody] BanRequestDTO dto)
    {
        await commentService.BanUserAsync(
            mapper.Map<BanModel>(dto));

        return Ok();
    }
}
