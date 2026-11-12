using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.Webhooks
{
    public class PayPalWebhookVerificationRequest
    {
        public string AuthAlgo { get; set; } = string.Empty;

        public string CertUrl { get; set; } = string.Empty;

        public string TransmissionId { get; set; } = string.Empty;

        public string TransmissionSig { get; set; } = string.Empty;

        public string TransmissionTime { get; set; } = string.Empty;

        public string WebhookId { get; set; } = string.Empty;

        public JsonElement WebhookEvent { get; set; }
    }
}
