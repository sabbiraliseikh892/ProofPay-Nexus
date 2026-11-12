using ProofPayNexus.Application.DTOs.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Services
{
    public interface IPayPalWebhookService
    {
        Task<bool> VerifyWebhookSignatureAsync(
            PayPalWebhookVerificationRequest request,
            CancellationToken cancellationToken = default);
        Task ProcessVerifiedWebhookAsync(
      JsonElement webhookEvent,
      CancellationToken cancellationToken = default);
    }
}
