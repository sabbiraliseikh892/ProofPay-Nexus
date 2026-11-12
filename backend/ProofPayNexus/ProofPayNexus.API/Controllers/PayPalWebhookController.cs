using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProofPayNexus.Application.DTOs.Webhooks;
using ProofPayNexus.Application.Interfaces.Services;
using System.Text.Json;

namespace ProofPayNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayPalWebhookController : ControllerBase
    {
        private readonly IPayPalWebhookService _webhookService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayPalWebhookController> _logger;

        public PayPalWebhookController(
            IPayPalWebhookService webhookService,
            IConfiguration configuration,
            ILogger<PayPalWebhookController> logger)
        {
            _webhookService = webhookService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook(
            [FromHeader(Name = "PAYPAL-AUTH-ALGO")] string authAlgo,
            [FromHeader(Name = "PAYPAL-CERT-URL")] string certUrl,
            [FromHeader(Name = "PAYPAL-TRANSMISSION-ID")] string transmissionId,
            [FromHeader(Name = "PAYPAL-TRANSMISSION-SIG")] string transmissionSig,
            [FromHeader(Name = "PAYPAL-TRANSMISSION-TIME")] string transmissionTime,
            [FromBody] JsonElement webhookEvent,
            CancellationToken cancellationToken)
        {
            try
            {
                string webhookId =
                    _configuration["PayPal:WebhookId"] ?? string.Empty;

                if (string.IsNullOrWhiteSpace(webhookId))
                {
                    _logger.LogError(
                        "PayPal WebhookId configuration is missing.");

                    return StatusCode(500, new
                    {
                        message = "Webhook configuration is missing."
                    });
                }

                var verificationRequest =
                    new PayPalWebhookVerificationRequest
                    {
                        AuthAlgo = authAlgo,
                        CertUrl = certUrl,
                        TransmissionId = transmissionId,
                        TransmissionSig = transmissionSig,
                        TransmissionTime = transmissionTime,
                        WebhookId = webhookId,
                        WebhookEvent = webhookEvent
                    };

                bool isVerified =
                    await _webhookService.VerifyWebhookSignatureAsync(
                        verificationRequest,
                        cancellationToken);

                if (!isVerified)
                {
                    _logger.LogWarning(
                        "PayPal webhook signature verification failed.");

                    return Unauthorized(new
                    {
                        message = "Invalid PayPal webhook signature."
                    });
                }

                string eventType = "UNKNOWN";

                if (webhookEvent.TryGetProperty(
                    "event_type",
                    out JsonElement eventTypeElement))
                {
                    eventType = eventTypeElement.GetString() ?? "UNKNOWN";
                }

                string eventId = string.Empty;

                if (webhookEvent.TryGetProperty(
                    "id",
                    out JsonElement eventIdElement))
                {
                    eventId = eventIdElement.GetString() ?? string.Empty;
                }

                _logger.LogInformation(
                    "Verified PayPal webhook received. Event: {EventType}, ID: {EventId}",
                    eventType,
                    eventId);

                // Event processing will be implemented in the next step.
                await _webhookService.ProcessVerifiedWebhookAsync(
     webhookEvent,
     cancellationToken);

                return Ok(new
                {
                    message = "PayPal webhook verified and processed successfully.",
                    eventId,
                    eventType
                });
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "PayPal webhook verification request failed.");

                return StatusCode(502, new
                {
                    message = "Unable to verify PayPal webhook."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error processing PayPal webhook.");

                return StatusCode(500, new
                {
                    message = "An unexpected webhook error occurred."
                });
            }
        }
    }
}
