using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Domain.Entities
{
    public class PaymentTransaction
    {
        public Guid Id { get; set; }

        public Guid PurchaseOrderId { get; set; }

        public Guid? PaymentAssessmentId { get; set; }

        public string TransactionReference { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; } = "USD";

        // PayPal identifiers
        public string? PayPalOrderId { get; set; }

        public string? PayPalAuthorizationId { get; set; }

        public string? PayPalCaptureId { get; set; }

        public PaymentStatus Status { get; set; }
            = PaymentStatus.Pending;

        // Human approval
        public ApprovalStatus ApprovalStatus { get; set; }
            = ApprovalStatus.Pending;

        public string? ReviewedBy { get; set; }

        public string? ReviewNotes { get; set; }

        public DateTime? ReviewedAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? CapturedAtUtc { get; set; }

        // Navigation properties
        public PurchaseOrder? PurchaseOrder { get; set; }

        public PaymentAssessment? PaymentAssessment { get; set; }
    }
}
