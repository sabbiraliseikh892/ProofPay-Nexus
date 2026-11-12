using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.Payments
{
    public class PayPalOrderResponse
    {
        public Guid PaymentTransactionId { get; set; }

        public string PayPalOrderId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string ApprovalUrl { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;
    }
}
