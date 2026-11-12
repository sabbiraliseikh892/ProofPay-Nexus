using Microsoft.Extensions.Configuration;
using ProofPayNexus.Application.DTOs.Webhooks;
using ProofPayNexus.Application.Interfaces.Repositories;
using ProofPayNexus.Application.Interfaces.Services;
using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProofPayNexus.Infrastructure.Webhooks
{
    public class PayPalWebhookService : IPayPalWebhookService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IPaymentTransactionRepository _transactionRepository;

        public PayPalWebhookService(
     HttpClient httpClient,
     IConfiguration configuration,
     IPaymentTransactionRepository transactionRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _transactionRepository = transactionRepository;
        }

        public async Task<bool> VerifyWebhookSignatureAsync(
            PayPalWebhookVerificationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.AuthAlgo) ||
                string.IsNullOrWhiteSpace(request.CertUrl) ||
                string.IsNullOrWhiteSpace(request.TransmissionId) ||
                string.IsNullOrWhiteSpace(request.TransmissionSig) ||
                string.IsNullOrWhiteSpace(request.TransmissionTime) ||
                string.IsNullOrWhiteSpace(request.WebhookId) ||
                request.WebhookEvent.ValueKind == JsonValueKind.Undefined ||
                request.WebhookEvent.ValueKind == JsonValueKind.Null)
            {
                throw new ArgumentException(
                    "Required PayPal webhook verification data is missing.");
            }

            string configuredWebhookId =
                _configuration["PayPal:WebhookId"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(configuredWebhookId) ||
                !string.Equals(
                    configuredWebhookId,
                    request.WebhookId,
                    StringComparison.Ordinal))
            {
                return false;
            }

            string accessToken =
                await GetAccessTokenAsync(cancellationToken);

            string baseUrl =
                _configuration["PayPal:BaseUrl"] ?? string.Empty;

            string requestJson = JsonSerializer.Serialize(new
            {
                auth_algo = request.AuthAlgo,
                cert_url = request.CertUrl,
                transmission_id = request.TransmissionId,
                transmission_sig = request.TransmissionSig,
                transmission_time = request.TransmissionTime,
                webhook_id = configuredWebhookId,
                webhook_event = request.WebhookEvent
            });

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"{baseUrl.TrimEnd('/')}/v1/notifications/verify-webhook-signature");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            httpRequest.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            httpRequest.Content = new StringContent(
                requestJson,
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            string responseContent =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"PayPal webhook verification failed. " +
                    $"HTTP Status: {(int)response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            using JsonDocument document =
                JsonDocument.Parse(responseContent);

            if (!document.RootElement.TryGetProperty(
                "verification_status",
                out JsonElement statusElement))
            {
                throw new InvalidOperationException(
                    "PayPal verification response is missing verification_status.");
            }

            return string.Equals(
                statusElement.GetString(),
                "SUCCESS",
                StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string> GetAccessTokenAsync(
            CancellationToken cancellationToken)
        {
            string clientId =
                _configuration["PayPal:ClientId"] ?? string.Empty;

            string secret =
                _configuration["PayPal:Secret"] ?? string.Empty;

            string baseUrl =
                _configuration["PayPal:BaseUrl"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(secret) ||
                string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "PayPal configuration is missing.");
            }

            string credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{clientId}:{secret}"));

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{baseUrl.TrimEnd('/')}/v1/oauth2/token");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            request.Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                { "grant_type", "client_credentials" }
                });

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            string responseContent =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"PayPal authentication failed. " +
                    $"HTTP Status: {(int)response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            using JsonDocument document =
                JsonDocument.Parse(responseContent);

            if (!document.RootElement.TryGetProperty(
                "access_token",
                out JsonElement tokenElement))
            {
                throw new InvalidOperationException(
                    "PayPal access token was not returned.");
            }

            string? accessToken = tokenElement.GetString();

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException(
                    "PayPal returned an empty access token.");
            }

            return accessToken;
        }


        public async Task ProcessVerifiedWebhookAsync(
        JsonElement webhookEvent,
        CancellationToken cancellationToken = default)
        {
            if (webhookEvent.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException(
                    "Invalid PayPal webhook event.");
            }

            // Extract event type
            string eventType = string.Empty;

            if (webhookEvent.TryGetProperty("event_type", out JsonElement eventTypeElement))
            {
                eventType = eventTypeElement.GetString() ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(eventType))
            {
                throw new InvalidOperationException(
                    "PayPal webhook event type is missing.");
            }

            // CHECKOUT.ORDER.APPROVED is handled by the PayPal return endpoint.
            // Do not look for supplementary_data.related_ids.order_id here.
            if (string.Equals(
                eventType,
                "CHECKOUT.ORDER.APPROVED",
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Extract resource
            if (!webhookEvent.TryGetProperty(
                "resource",
                out JsonElement resource))
            {
                return;
            }

            if (resource.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            // Extract PayPal Order ID
            string? payPalOrderId = null;

            if (resource.TryGetProperty(
                "supplementary_data",
                out JsonElement supplementaryData))
            {
                if (supplementaryData.TryGetProperty(
                    "related_ids",
                    out JsonElement relatedIds))
                {
                    if (relatedIds.TryGetProperty(
                        "order_id",
                        out JsonElement orderIdElement))
                    {
                        payPalOrderId = orderIdElement.GetString();
                    }
                }
            }

            // Some event types may not contain an Order ID.
            // Ignore them instead of throwing an exception.
            if (string.IsNullOrWhiteSpace(payPalOrderId))
            {
                return;
            }

            // Find local payment transaction
            var transaction = await _transactionRepository
                .GetByPayPalOrderIdAsync(
                    payPalOrderId,
                    cancellationToken);

            if (transaction == null)
            {
                return;
            }

            // Extract PayPal resource ID
            string? resourceId = null;

            if (resource.TryGetProperty(
                "id",
                out JsonElement resourceIdElement))
            {
                resourceId = resourceIdElement.GetString();
            }

            // Process authorization-created event
            if (string.Equals(
                eventType,
                "PAYMENT.AUTHORIZATION.CREATED",
                StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    return;
                }

                // Avoid overwriting an already captured transaction.
                if (transaction.Status == PaymentStatus.Captured)
                {
                    return;
                }

                transaction.PayPalAuthorizationId = resourceId;
                transaction.Status = PaymentStatus.Authorized;
                transaction.ApprovalStatus = ApprovalStatus.Approved;

                await _transactionRepository.SaveChangesAsync(
                    cancellationToken);

                return;
            }

            // Process successful capture event
            if (string.Equals(
                eventType,
                "PAYMENT.CAPTURE.COMPLETED",
                StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    return;
                }

                // Idempotency: do not process an already captured payment again.
                if (transaction.Status == PaymentStatus.Captured)
                {
                    return;
                }

                transaction.PayPalCaptureId = resourceId;
                transaction.Status = PaymentStatus.Captured;
                transaction.CapturedAtUtc = DateTime.UtcNow;

                await _transactionRepository.SaveChangesAsync(
                    cancellationToken);

                return;
            }

            // Process denied capture event
            if (string.Equals(
                eventType,
                "PAYMENT.CAPTURE.DENIED",
                StringComparison.OrdinalIgnoreCase))
            {
                // Never overwrite a successful capture.
                if (transaction.Status == PaymentStatus.Captured)
                {
                    return;
                }

                transaction.Status = PaymentStatus.Failed;

                await _transactionRepository.SaveChangesAsync(
                    cancellationToken);

                return;
            }

            // Process voided authorization event
            if (string.Equals(
                eventType,
                "PAYMENT.AUTHORIZATION.VOIDED",
                StringComparison.OrdinalIgnoreCase))
            {
                if (transaction.Status == PaymentStatus.Captured)
                {
                    return;
                }

                transaction.Status = PaymentStatus.Voided;

                await _transactionRepository.SaveChangesAsync(
                    cancellationToken);

                return;
            }

            // Other webhook events are currently not processed by the MVP.
            return;
        }

    }
}
