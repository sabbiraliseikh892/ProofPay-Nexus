
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ProofPayNexus.Application.Interfaces;
using ProofPayNexus.Application.Interfaces.Services;

namespace ProofPayNexus.Infrastructure.Payments;

public class PayPalAuthService : IPayPalAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public PayPalAuthService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GetAccessTokenAsync(
        CancellationToken cancellationToken = default)
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
                "access_token", out JsonElement tokenElement))
        {
            throw new InvalidOperationException(
                "PayPal response does not contain an access token.");
        }

        string? accessToken = tokenElement.GetString();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                "PayPal returned an empty access token.");
        }

        return accessToken;
    }
}
