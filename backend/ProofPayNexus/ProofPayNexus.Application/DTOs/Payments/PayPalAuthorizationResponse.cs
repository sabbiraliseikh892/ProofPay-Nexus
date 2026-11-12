using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.Payments
{
    public class PayPalAuthorizationResponse
    {
        public Guid PaymentTransactionId { get; set; }

        public string PayPalOrderId { get; set; } = string.Empty;

        public string AuthorizationId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
