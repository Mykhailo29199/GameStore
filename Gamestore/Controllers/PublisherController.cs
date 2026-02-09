using AutoMapper;
using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services.Interaces;
using Gamestore.WebAPI.DTOs.PublisherDTO;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class PublisherController(IPublisherService publisherService, IMapper mapper) : ControllerBase
{
    private readonly IPublisherService _publisherService = publisherService;
    private readonly IMapper _mapper = mapper;

    [HttpPost("publisher")]
    public async Task<IActionResult> CreatePublisherAsync([FromBody] CreatePublisherDTO requestPostDTO)
    {
        var publisherModel = _mapper.Map<PublisherModel>(requestPostDTO);
        await _publisherService.CreatePublisherAsync(publisherModel);

        return NoContent();
    }

    [HttpGet("publisher/{companyName}")]
    public async Task<IActionResult> GetPublisherByCompanyNameAsync([FromRoute] string companyName)
    {
        PublisherModel publisherModel = await _publisherService.GetPublisherByCompanyNameAsync(companyName);
        PublisherWithIdDTO responseGetDTO = _mapper.Map<PublisherWithIdDTO>(publisherModel);

        return Ok(responseGetDTO);
    }

    [HttpGet("publishers")]
    public async Task<IActionResult> GetPublishersAsync()
    {
        List<PublisherModel> publisherModels = await _publisherService.GetPublishersAsync();
        List<PublisherWithIdDTO> responseGetDTOs = _mapper.Map<List<PublisherWithIdDTO>>(publisherModels);

        return Ok(responseGetDTOs);
    }

    [HttpPut("publisher")]
    public async Task<IActionResult> UpdatePublisherAsync([FromBody] UpdatePublisherDTO requestPutDTO)
    {
        var publisherModel = _mapper.Map<PublisherModel>(requestPutDTO);
        await _publisherService.UpdatePublisherAsync(publisherModel);

        return NoContent();
    }

    [HttpDelete("publisher/{publisherId}")]
    public async Task<IActionResult> DeletePublisherAsync([FromRoute] Guid publisherId)
    {
        await _publisherService.DeletePublisherAsync(publisherId);

        return NoContent();
    }

    [HttpGet("games/publisher/{companyname}")]
    public async Task<IActionResult> GetGamesByPublisherNameAsync([FromRoute] string companyname)
    {
        var games = await _publisherService.GetGamesByPublisherNameAsync(companyname);
        return Ok(games);
    }

    [HttpGet("games/publisher/{id:guid}")]
    public async Task<IActionResult> GetGamesByPublisherIdAsync([FromRoute] Guid id)
    {
        var games = await _publisherService.GetGamesByPublisherIdAsync(id);
        return Ok(games);
    }
}
