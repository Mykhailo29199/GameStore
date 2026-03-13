using AutoMapper;
using BusinessLogicLayer.Models.Payment;
using BusinessLogicLayer.Services.Interaces;
using Gamestore.WebAPI.DTOs.PaymentDTO;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(IPaymentService paymentService, IMapper mapper) : ControllerBase
{
    private readonly IPaymentService _paymentService = paymentService;
    private readonly IMapper _mapper = mapper;

    //[HttpPost("payment")]
    //public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDTO request)
    //{
    //    var model = _mapper.Map<PaymentRequestModel>(request);

    //    var result = await _paymentService.ProcessPayment(model);

    //    if (!result.IsSuccess)
    //    {
    //        return BadRequest(result.Message);
    //    }

    //    if (result.FileBytes != null)
    //    {
    //        return File(result.FileBytes, result.ContentType!, result.FileName);
    //    }

    //    if (result.Data != null)
    //    {
    //        return Ok(result.Data);
    //    }

    //    return Ok();
    //}

    [HttpPost("payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDTO request)
    {
        var requestModel = _mapper.Map<PaymentRequestModel>(request);
        var responseModel = await _paymentService.ProcessPayment(requestModel);

        if (responseModel.Status != "Success")
        {
            return BadRequest(responseModel.Message);
        }

        if (responseModel.Method == "Bank" && responseModel.BankResponseModel.FileBytes != null)
        {
            return File(responseModel.BankResponseModel.FileBytes, "application/pdf", $"invoice.pdf");
        }

        return responseModel.Method == "IBox terminal" && responseModel.IBoxResponseModel != null
            ? Ok(responseModel.IBoxResponseModel)
            : Ok();
    }
}
