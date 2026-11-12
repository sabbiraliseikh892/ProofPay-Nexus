using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProofPayNexus.Application.Interfaces.Services;

namespace ProofPayNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayPalAuthController : ControllerBase
    {
        private readonly IPayPalAuthService _payPalAuthService;
        private readonly ILogger<PayPalAuthController> _logger;

        public PayPalAuthController(
            IPayPalAuthService payPalAuthService,
            ILogger<PayPalAuthController> logger)
        {
            _payPalAuthService = payPalAuthService;
            _logger = logger;
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestAuthentication(
            CancellationToken cancellationToken)
        {
            try
            {
                string accessToken =
                    await _payPalAuthService.GetAccessTokenAsync(
                        cancellationToken);

                return Ok(new
                {
                    authenticated = !string.IsNullOrWhiteSpace(accessToken),
                    message = "PayPal Sandbox authentication successful."
                });
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "PayPal Sandbox authentication failed.");

                return StatusCode(502, new
                {
                    authenticated = false,
                    message = "PayPal Sandbox authentication failed. Check API logs."
                });
            }
        }
    }
}
