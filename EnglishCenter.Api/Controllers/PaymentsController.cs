using EnglishCenter.Application.Features.Payments;
using EnglishCenter.Application.Features.Payments.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenter.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentsController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] GetPaymentsPagingRequestDto request)
        {
            var result = await _paymentService.GetPagedAsync(request);
            return Ok(result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _paymentService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequestDto request)
        {
            var id = await _paymentService.CreateAsync(request);
            return Ok(new { Id = id });
        }
    }
}
