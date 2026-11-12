using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProofPayNexus.Application.DTOs.Payments;
using ProofPayNexus.Application.Interfaces.Services;

namespace ProofPayNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayPalPaymentController : ControllerBase
    {
        private readonly IPayPalPaymentService _payPalPaymentService;
        private readonly ILogger<PayPalPaymentController> _logger;

        public PayPalPaymentController(
            IPayPalPaymentService payPalPaymentService,
            ILogger<PayPalPaymentController> logger)
        {
            _payPalPaymentService = payPalPaymentService;
            _logger = logger;
        }
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder(
       [FromBody] CreatePayPalOrderRequest request,
       CancellationToken cancellationToken)
        {
            try
            {
                if (request.PurchaseOrderId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message = "A valid purchase order ID is required."
                    });
                }

                PayPalOrderResponse result =
                    await _payPalPaymentService.CreateAuthorizationOrderAsync(
                        request.PurchaseOrderId,
                        cancellationToken);

                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "PayPal order creation failed.");

                return StatusCode(502, new
                {
                    message = "PayPal order creation failed. Check API logs."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating PayPal order.");

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred."
                });
            }
        }


        [HttpGet("return")]
        public async Task<IActionResult> ReturnFromPayPal(
            [FromQuery] string token,
            [FromQuery] string? PayerID,
            CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new
                    {
                        message = "PayPal order token is required."
                    });
                }

                PayPalAuthorizationResponse result =
                    await _payPalPaymentService.AuthorizeOrderAsync(
                        token,
                        cancellationToken);

                _logger.LogInformation(
                    "PayPal order {PayPalOrderId} authorized successfully. Authorization ID: {AuthorizationId}",
                    result.PayPalOrderId,
                    result.AuthorizationId);

                return Ok(new
                {
                    message = "PayPal payment authorized successfully.",
                    payment = result
                });
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "PayPal authorization failed for order {PayPalOrderId}",
                    token);

                return StatusCode(502, new
                {
                    message = "PayPal authorization failed. Check API logs."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error processing PayPal return.");

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred."
                });
            }
        }


        [HttpGet("cancel")]
        public IActionResult CancelFromPayPal(
            [FromQuery] string? token)
        {
            _logger.LogInformation(
                "Buyer cancelled PayPal checkout. PayPal Order ID: {PayPalOrderId}",
                token);

            return Ok(new
            {
                status = "CANCELLED",
                message = "PayPal checkout was cancelled by the buyer.",
                payPalOrderId = token
            });
        }


        [HttpPost("capture/{paymentTransactionId:guid}")]
        public async Task<IActionResult> CapturePayment(
            Guid paymentTransactionId,
            CancellationToken cancellationToken)
        {
            try
            {
                if (paymentTransactionId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        message = "A valid payment transaction ID is required."
                    });
                }

                PayPalCaptureResponse result =
                    await _payPalPaymentService.CaptureAuthorizedPaymentAsync(
                        paymentTransactionId,
                        cancellationToken);

                _logger.LogInformation(
                    "PayPal payment captured. Transaction: {TransactionId}, Capture ID: {CaptureId}",
                    result.PaymentTransactionId,
                    result.CaptureId);

                return Ok(new
                {
                    message = "PayPal payment captured successfully.",
                    payment = result
                });
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "PayPal capture failed for transaction {TransactionId}",
                    paymentTransactionId);

                return StatusCode(502, new
                {
                    message = "PayPal capture failed. Check API logs."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error capturing PayPal payment.");

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred."
                });
            }
        }



    }
}
