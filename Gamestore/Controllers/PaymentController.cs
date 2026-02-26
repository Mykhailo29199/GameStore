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

    [HttpPost("payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDTO request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

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

        if (responseModel.Method == "IBox terminal" && responseModel.IBoxResponseModel != null)
        {
            return Ok(responseModel.IBoxResponseModel);
        }

        return Ok();
    }
}
