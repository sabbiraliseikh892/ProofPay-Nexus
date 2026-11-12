using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Domain.Enums
{
    public enum AuditAction
    {
        PurchaseOrderCreated = 1,
        PurchaseOrderUpdated = 2,
        EvidenceUploaded = 3,
        EvidenceAnalyzed = 4,
        DiscrepancyDetected = 5,
        DiscrepancyResolved = 6,
        PaymentAssessed = 7,
        PaymentApproved = 8,
        PaymentRejected = 9,
        PayPalOrderCreated = 10,
        PaymentAuthorized = 11,
        PaymentCaptured = 12,
        PaymentFailed = 13,
        WebhookReceived = 14,
        DiscrepancyReviewStarted = 15,
        DiscrepancyAccepted = 16,
        DiscrepancyRejected = 17
    }
}
