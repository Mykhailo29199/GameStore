using AutoMapper;
using BusinessLogicLayer.Models.Payment;
using BusinessLogicLayer.Services.Interaces;
using GameStore.Shared.Authorization;
using GameStore.Shared.Enums.PaymentEnums;
using Gamestore.WebAPI.DTOs.PaymentDTO;
using Gamestore.WebAPI.DTOs.PaymentDTO.PaymentResponseDTOs;
using Gamestore.WebAPI.Security;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.WebAPI.Controllers;

[ApiController]
[Route("api/")]
public class PaymentController(IPaymentService paymentService, IMapper mapper) : ControllerBase
{
    private readonly IPaymentService _paymentService = paymentService;
    private readonly IMapper _mapper = mapper;

    [HasPermission(Permissions.BuyGame)]

    [HttpPost("orders/payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDTO request)
    {
        var requestModel = _mapper.Map<PaymentRequestModel>(request);
        var responseModel = await _paymentService.ProcessPayment(requestModel);

        return responseModel.Method switch
        {
            PaymentOptions.Bank when responseModel.BankResponseModel.FileBytes != null =>
                File(responseModel.BankResponseModel.FileBytes, "application/pdf", "invoice.pdf"),
            PaymentOptions.IBoxTerminal when responseModel.IBoxResponseModel != null =>
                Ok(responseModel.IBoxResponseModel),
            PaymentOptions.Visa =>
        Ok(new { message = "Payment completed successfully via Visa." }),
            _ => BadRequest("Unknown payment method"),
        };
    }

    [HasPermission(Permissions.BuyGame)]

    [HttpGet("orders/payment-methods")]
    public IActionResult GetPaymentMethods()
    {
        var paymentMethods = _mapper.Map<List<PaymentMethodResponseDTO>>(_paymentService.GetPaymentMethods());

        return Ok(new { paymentMethods });
    }
}
