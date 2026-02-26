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

    // if (!ModelState.IsValid)
    // {
    //    return BadRequest(ModelState);
    // }

    // PaymentResponseDTO response;

    // switch (request.Method)
    // {
    //    case "Visa":
    //        var visaModel = _mapper.Map<VisaPaymentModel>(request.Model);
    //        var visaResponse = await _paymentService.PayWithVisa(visaModel);
    //        response = _mapper.Map<PaymentResponseDTO>(visaResponse);
    //        break;

    // case "IBox terminal":
    //        var iboxModel = _mapper.Map<IBoxPaymentModel>(request.IBoxModel);
    //        var iboxResponse = await _paymentService.PayWithIBox(iboxModel);
    //        response = _mapper.Map<PaymentResponseDTO>(iboxResponse);
    //        break;

    // case "Bank":
    //    var bankModel = _mapper.Map<BankPaymentModel>(request.BankModel);
    //    var bankResponse = await _paymentService.PayWithBank(bankModel);
    //    response = _mapper.Map<PaymentResponseDTO>(bankResponse);
    //    break;
    //        default:
    //                return BadRequest(new { message = "Unsupported payment method." });
    //    }

    // if (response.Status == "Success")
    //        {
    //            await _paymentService.MarkOrderPaid(request.OrderId);
    //            return Ok(response);
    // }
    //        else
    //        {
    //            await _paymentService.MarkOrderCancelled(request.OrderId);
    //            return BadRequest(response);
    //        }
    //    }

    //[HttpPost("visa")]
    //public async Task<IActionResult> PayWithVisa([FromBody] VisaPaymentDTO request)
    //{
    //    var payment = _mapper.Map<VisaPaymentModel>(request);
    //    var response = await _paymentService.PayWithVisa(payment);
    //    var dto = _mapper.Map<PaymentResponseDTO>(response);

    //    return dto.Status == "Success"
    //      ? Ok(new { message = "Оплата Visa успішна", data = dto })
    //      : StatusCode(402, new { message = "Недостатньо коштів для оплати Visa", data = dto });
    //}

    //[HttpPost("ibox")]
    //public async Task<IActionResult> PayWithIBox([FromBody] IBoxPaymentDTO request)
    //{
    //    var payment = _mapper.Map<IBoxPaymentModel>(request);
    //    var response = await _paymentService.PayWithIBox(payment);
    //    var dto = _mapper.Map<PaymentResponseDTO>(response);

    //    return dto.Status == "Success"
    //        ? Ok(new { message = "Оплата iBox успішна", data = dto })
    //        : BadRequest(new { message = "Помилка при оплаті через iBox", data = dto });
    //}

    //[HttpPost("bank")]
    //public async Task<IActionResult> PayWithBank([FromBody] BankPaymentDTO request)
    //{
    //    var payment = _mapper.Map<BankPaymentModel>(request);
    //    var response = await _paymentService.PayWithBank(payment);
    //    var dto = _mapper.Map<PaymentResponseDTO>(response);

    //    return dto.Status == "Success"
    //        ? Ok(new { message = "Оплата через банк успішна", data = dto })
    //        : StatusCode(402, new { message = "Інвойс недійсний або оплата не пройшла", data = dto });
    //}
}
