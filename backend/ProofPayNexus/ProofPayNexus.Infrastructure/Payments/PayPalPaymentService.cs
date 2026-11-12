using Microsoft.Extensions.Configuration;
using ProofPayNexus.Application.DTOs.Payments;
using ProofPayNexus.Application.Interfaces.Repositories;
using ProofPayNexus.Application.Interfaces.Services;
using ProofPayNexus.Domain.Entities;
using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProofPayNexus.Infrastructure.Payments
{
    public class PayPalPaymentService : IPayPalPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IPayPalAuthService _payPalAuthService;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IPaymentAssessmentRepository _assessmentRepository;
        private readonly IPaymentTransactionRepository _transactionRepository;
        public PayPalPaymentService(
        HttpClient httpClient,
        IConfiguration configuration,
        IPayPalAuthService payPalAuthService,
        IPurchaseOrderRepository purchaseOrderRepository,
        IPaymentAssessmentRepository assessmentRepository,
        IPaymentTransactionRepository transactionRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _payPalAuthService = payPalAuthService;
            _purchaseOrderRepository = purchaseOrderRepository;
            _assessmentRepository = assessmentRepository;
            _transactionRepository = transactionRepository;
        }
        public async Task<PayPalOrderResponse> CreateAuthorizationOrderAsync(
       Guid purchaseOrderId,
       CancellationToken cancellationToken = default)
        {
            if (purchaseOrderId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid purchase order ID is required.");
            }

            var purchaseOrder =
                await _purchaseOrderRepository.GetByIdAsync(
                    purchaseOrderId,
                    cancellationToken);

            if (purchaseOrder == null)
            {
                throw new KeyNotFoundException(
                    "Purchase order not found.");
            }

            var assessment =
                await _assessmentRepository.GetLatestByPurchaseOrderIdAsync(
                    purchaseOrderId,
                    cancellationToken);

            if (assessment == null)
            {
                throw new InvalidOperationException(
                    "Payment risk assessment is required before payment.");
            }

            if (assessment.Recommendation != PaymentRecommendation.Approve)
            {
                throw new InvalidOperationException(
                    "Payment cannot proceed. Risk recommendation: " +
                    assessment.Recommendation);
            }

            if (purchaseOrder.TotalAmount <= 0)
            {
                throw new InvalidOperationException(
                    "Purchase order amount must be greater than zero.");
            }

            string accessToken =
                await _payPalAuthService.GetAccessTokenAsync(
                    cancellationToken);

            string baseUrl =
                _configuration["PayPal:BaseUrl"] ?? string.Empty;

            string returnUrl =
                _configuration["PayPal:ReturnUrl"] ?? string.Empty;

            string cancelUrl =
                _configuration["PayPal:CancelUrl"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(returnUrl) ||
                string.IsNullOrWhiteSpace(cancelUrl))
            {
                throw new InvalidOperationException(
                    "PayPal return and cancel URLs are required.");
            }

            string transactionReference =
                Guid.NewGuid().ToString("N");

            var transaction = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrder.Id,
                PaymentAssessmentId = assessment.Id,
                TransactionReference = transactionReference,
                Amount = purchaseOrder.TotalAmount,
                CurrencyCode = purchaseOrder.CurrencyCode,
                Status = PaymentStatus.Pending,
                ApprovalStatus = ApprovalStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            };

            string requestJson = JsonSerializer.Serialize(new
            {
                intent = "AUTHORIZE",

                purchase_units = new[]
                {
                new
                {
                    reference_id = transactionReference,
                    invoice_id = transactionReference,
                    description =
                        $"Payment for {purchaseOrder.PurchaseOrderNumber}",

                    amount = new
                    {
                        currency_code = purchaseOrder.CurrencyCode,
                        value = purchaseOrder.TotalAmount.ToString(
                            "F2",
                            System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
            },

                payment_source = (object?)null,

                application_context = new
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl,
                    user_action = "PAY_NOW",
                    shipping_preference = "NO_SHIPPING"
                }
            });

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{baseUrl.TrimEnd('/')}/v2/checkout/orders");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = new StringContent(
                requestJson,
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            string responseContent =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"PayPal order creation failed. " +
                    $"HTTP Status: {(int)response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            using JsonDocument document =
                JsonDocument.Parse(responseContent);

            JsonElement root = document.RootElement;

            string? payPalOrderId = null;
            string? status = null;
            string? approvalUrl = null;

            if (root.TryGetProperty("id", out JsonElement idElement))
            {
                payPalOrderId = idElement.GetString();
            }

            if (root.TryGetProperty("status", out JsonElement statusElement))
            {
                status = statusElement.GetString();
            }

            if (root.TryGetProperty("links", out JsonElement linksElement))
            {
                foreach (JsonElement link in linksElement.EnumerateArray())
                {
                    if (link.TryGetProperty("rel", out JsonElement relElement) &&
                        string.Equals(
                            relElement.GetString(),
                            "approve",
                            StringComparison.OrdinalIgnoreCase) &&
                        link.TryGetProperty("href", out JsonElement hrefElement))
                    {
                        approvalUrl = hrefElement.GetString();
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(payPalOrderId) ||
                string.IsNullOrWhiteSpace(approvalUrl))
            {
                throw new InvalidOperationException(
                    "PayPal response is missing the order ID or approval URL.");
            }

            transaction.PayPalOrderId = payPalOrderId;
            transaction.Status = PaymentStatus.Created;

            await _transactionRepository.AddAsync(
                transaction,
                cancellationToken);

            await _transactionRepository.SaveChangesAsync(
                cancellationToken);

            return new PayPalOrderResponse
            {
                PaymentTransactionId = transaction.Id,
                PayPalOrderId = payPalOrderId,
                Status = status ?? "CREATED",
                ApprovalUrl = approvalUrl,
                Amount = transaction.Amount,
                CurrencyCode = transaction.CurrencyCode
            };
        }




        public async Task<PayPalAuthorizationResponse> AuthorizeOrderAsync(
            string payPalOrderId,
            CancellationToken cancellationToken = default)
        {
            // 1. Validate PayPal order ID.
            if (string.IsNullOrWhiteSpace(payPalOrderId))
            {
                throw new ArgumentException(
                    "A valid PayPal order ID is required.",
                    nameof(payPalOrderId));
            }

            // 2. Find local payment transaction.
            var transaction =
                await _transactionRepository.GetByPayPalOrderIdAsync(
                    payPalOrderId,
                    cancellationToken);

            if (transaction == null)
            {
                throw new KeyNotFoundException(
                    "Payment transaction not found for this PayPal order.");
            }

            // 3. Idempotency: return existing authorization.
            if (transaction.Status == PaymentStatus.Authorized &&
                !string.IsNullOrWhiteSpace(transaction.PayPalAuthorizationId))
            {
                return new PayPalAuthorizationResponse
                {
                    PaymentTransactionId = transaction.Id,
                    PayPalOrderId = payPalOrderId,
                    AuthorizationId = transaction.PayPalAuthorizationId,
                    Status = "AUTHORIZED"
                };
            }

            // 4. Validate transaction status.
            if (transaction.Status != PaymentStatus.Created &&
                transaction.Status != PaymentStatus.BuyerApproved)
            {
                throw new InvalidOperationException(
                    $"Payment cannot be authorized from status {transaction.Status}.");
            }

            // 5. Validate PayPal configuration.
            string baseUrl =
                _configuration["PayPal:BaseUrl"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "PayPal BaseUrl configuration is missing.");
            }

            // 6. Get PayPal access token.
            string accessToken =
                await _payPalAuthService.GetAccessTokenAsync(
                    cancellationToken);

            string encodedOrderId =
                Uri.EscapeDataString(payPalOrderId);

            // 7. Verify PayPal order.
            using var verifyRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"{baseUrl.TrimEnd('/')}/v2/checkout/orders/{encodedOrderId}");

            verifyRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            verifyRequest.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            using var verifyResponse = await _httpClient.SendAsync(
                verifyRequest,
                cancellationToken);

            string verifyContent =
                await verifyResponse.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!verifyResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"PayPal order verification failed. " +
                    $"HTTP Status: {(int)verifyResponse.StatusCode}. " +
                    $"Response: {verifyContent}");
            }

            // 8. Check PayPal order status.
            using JsonDocument verifyDocument =
                JsonDocument.Parse(verifyContent);

            JsonElement verifyRoot = verifyDocument.RootElement;

            string? orderStatus = null;

            if (verifyRoot.TryGetProperty(
                "status",
                out JsonElement orderStatusElement))
            {
                orderStatus = orderStatusElement.GetString();
            }

            if (!string.Equals(
                orderStatus,
                "APPROVED",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"PayPal order is not approved. Current status: {orderStatus ?? "Unknown"}.");
            }

            // 9. Update local transaction to BuyerApproved.
            if (transaction.Status == PaymentStatus.Created)
            {
                transaction.Status = PaymentStatus.BuyerApproved;
                transaction.ApprovalStatus = ApprovalStatus.Approved;

                await _transactionRepository.SaveChangesAsync(
                    cancellationToken);
            }

            // 10. Prepare PayPal authorization request.
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{baseUrl.TrimEnd('/')}/v2/checkout/orders/{encodedOrderId}/authorize");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Stable request ID for safe retries.
            request.Headers.Add(
                "PayPal-Request-Id",
                transaction.TransactionReference);

            // Explicit JSON content type.
            request.Content = new StringContent(
                "{}",
                Encoding.UTF8,
                "application/json");

            // 11. Send authorization request.
            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            string responseContent =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"PayPal authorization failed. " +
                    $"HTTP Status: {(int)response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            // 12. Extract authorization details.
            using JsonDocument document =
                JsonDocument.Parse(responseContent);

            JsonElement root = document.RootElement;

            string? authorizationId = null;
            string? authorizationStatus = null;

            if (root.TryGetProperty(
                "purchase_units",
                out JsonElement purchaseUnits) &&
                purchaseUnits.ValueKind == JsonValueKind.Array &&
                purchaseUnits.GetArrayLength() > 0)
            {
                JsonElement firstPurchaseUnit = purchaseUnits[0];

                if (firstPurchaseUnit.TryGetProperty(
                    "payments",
                    out JsonElement payments) &&
                    payments.ValueKind == JsonValueKind.Object &&
                    payments.TryGetProperty(
                        "authorizations",
                        out JsonElement authorizations) &&
                    authorizations.ValueKind == JsonValueKind.Array &&
                    authorizations.GetArrayLength() > 0)
                {
                    JsonElement authorization = authorizations[0];

                    if (authorization.TryGetProperty(
                        "id",
                        out JsonElement idElement))
                    {
                        authorizationId = idElement.GetString();
                    }

                    if (authorization.TryGetProperty(
                        "status",
                        out JsonElement statusElement))
                    {
                        authorizationStatus = statusElement.GetString();
                    }
                }
            }

            // 13. Validate authorization response.
            if (string.IsNullOrWhiteSpace(authorizationId) ||
                !string.Equals(
                    authorizationStatus,
                    "CREATED",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"PayPal returned an unexpected authorization response. " +
                    $"Authorization status: {authorizationStatus ?? "Unknown"}. " +
                    $"Response: {responseContent}");
            }

            // 14. Update local transaction.
            transaction.PayPalAuthorizationId = authorizationId;
            transaction.Status = PaymentStatus.Authorized;
            transaction.ApprovalStatus = ApprovalStatus.Approved;

            await _transactionRepository.SaveChangesAsync(
                cancellationToken);

            // 15. Return authorization response.
            return new PayPalAuthorizationResponse
            {
                PaymentTransactionId = transaction.Id,
                PayPalOrderId = payPalOrderId,
                AuthorizationId = authorizationId,
                Status = "AUTHORIZED"
            };
        }


        public async Task<PayPalCaptureResponse> CaptureAuthorizedPaymentAsync(
            Guid paymentTransactionId,
            CancellationToken cancellationToken = default)
        {
            // 1. Validate transaction ID.
            if (paymentTransactionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid payment transaction ID is required.");
            }

            // 2. Find local payment transaction.
            var transaction = await _transactionRepository.GetByIdAsync(
                paymentTransactionId,
                cancellationToken);

            if (transaction == null)
            {
                throw new KeyNotFoundException(
                    "Payment transaction not found.");
            }

            // 3. Return existing result if already captured.
            if (transaction.Status == PaymentStatus.Captured &&
                !string.IsNullOrWhiteSpace(transaction.PayPalCaptureId))
            {
                return new PayPalCaptureResponse
                {
                    PaymentTransactionId = transaction.Id,
                    PayPalOrderId = transaction.PayPalOrderId ?? string.Empty,
                    AuthorizationId = transaction.PayPalAuthorizationId ?? string.Empty,
                    CaptureId = transaction.PayPalCaptureId,
                    Status = "COMPLETED",
                    Amount = transaction.Amount,
                    CurrencyCode = transaction.CurrencyCode,
                    CapturedAtUtc = transaction.CapturedAtUtc ?? DateTime.UtcNow
                };
            }

            // 4. Validate authorization.
            if (string.IsNullOrWhiteSpace(transaction.PayPalAuthorizationId))
            {
                throw new InvalidOperationException(
                    "PayPal authorization ID is missing.");
            }

            if (transaction.Status != PaymentStatus.Authorized &&
                transaction.Status != PaymentStatus.CapturePending)
            {
                throw new InvalidOperationException(
                    $"Payment cannot be captured from status {transaction.Status}.");
            }

            // 5. Get PayPal access token.
            string accessToken =
                await _payPalAuthService.GetAccessTokenAsync(
                    cancellationToken);

            // 6. Read PayPal configuration.
            string baseUrl =
                _configuration["PayPal:BaseUrl"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "PayPal BaseUrl configuration is missing.");
            }

            // 7. Mark capture as pending before calling PayPal.
            if (transaction.Status == PaymentStatus.Authorized)
            {
                transaction.Status = PaymentStatus.CapturePending;

                await _transactionRepository.SaveChangesAsync(
                    cancellationToken);
            }

            string encodedAuthorizationId =
                Uri.EscapeDataString(transaction.PayPalAuthorizationId);

            // 8. Create capture request.
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{baseUrl.TrimEnd('/')}/v2/payments/authorizations/{encodedAuthorizationId}/capture");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Stable ID ensures safe retries of this capture operation.
            request.Headers.Add(
                "PayPal-Request-Id",
                transaction.Id.ToString());

            request.Content = new StringContent(
                "{}",
                Encoding.UTF8,
                "application/json");

            // 9. Send request.
            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            string responseContent =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"PayPal capture failed. " +
                    $"HTTP Status: {(int)response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            // 10. Parse PayPal response.
            using JsonDocument document =
                JsonDocument.Parse(responseContent);

            JsonElement root = document.RootElement;

            string? captureId = null;
            string? captureStatus = null;
            decimal capturedAmount = transaction.Amount;
            string currencyCode = transaction.CurrencyCode;

            if (root.TryGetProperty("id", out JsonElement idElement))
            {
                captureId = idElement.GetString();
            }

            if (root.TryGetProperty("status", out JsonElement statusElement))
            {
                captureStatus = statusElement.GetString();
            }

            if (root.TryGetProperty("amount", out JsonElement amountElement))
            {
                if (amountElement.TryGetProperty(
                    "value", out JsonElement valueElement))
                {
                    decimal.TryParse(
                        valueElement.GetString(),
                        System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out capturedAmount);
                }

                if (amountElement.TryGetProperty(
                    "currency_code", out JsonElement currencyElement))
                {
                    currencyCode =
                        currencyElement.GetString() ?? currencyCode;
                }
            }

            // 11. Validate capture response.
            if (string.IsNullOrWhiteSpace(captureId) ||
                !string.Equals(
                    captureStatus,
                    "COMPLETED",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"PayPal capture was not completed. Status: {captureStatus ?? "Unknown"}.");
            }

            // 12. Update local transaction.
            transaction.PayPalCaptureId = captureId;
            transaction.Status = PaymentStatus.Captured;
            transaction.CapturedAtUtc = DateTime.UtcNow;

            await _transactionRepository.SaveChangesAsync(
                cancellationToken);

            // 13. Return capture response.
            return new PayPalCaptureResponse
            {
                PaymentTransactionId = transaction.Id,
                PayPalOrderId = transaction.PayPalOrderId ?? string.Empty,
                AuthorizationId = transaction.PayPalAuthorizationId,
                CaptureId = captureId,
                Status = captureStatus,
                Amount = capturedAmount,
                CurrencyCode = currencyCode,
                CapturedAtUtc = transaction.CapturedAtUtc.Value
            };
        }





    }
}
