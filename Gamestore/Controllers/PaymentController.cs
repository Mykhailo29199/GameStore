using AutoMapper;
using BusinessLogicLayer.Models.Payment;
using BusinessLogicLayer.Services.Interaces;
using Gamestore.WebAPI.DTOs.PaymentDTO;
using Gamestore.WebAPI.DTOs.PaymentDTO.PaymentResponseDTOs;
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
        var requestModel = _mapper.Map<PaymentRequestModel>(request);
        var responseModel = await _paymentService.ProcessPayment(requestModel);

        return responseModel.Method switch
        {
            "Bank" when responseModel.BankResponseModel.FileBytes != null =>
                File(responseModel.BankResponseModel.FileBytes, "application/pdf", $"invoice.pdf"),
            "IBox terminal" when responseModel.IBoxResponseModel != null =>
                Ok(responseModel.IBoxResponseModel),
            _ => Ok(),
        };
    }

    [HttpGet("orders/payment-methods")]
    public IActionResult GetPaymentMethods()
    {
        var paymentMethods = _paymentService.GetPaymentMethods();

        var result = _mapper.Map<IEnumerable<PaymentMethodResponseDTO>>(paymentMethods);

        return Ok(result);
    }
}
