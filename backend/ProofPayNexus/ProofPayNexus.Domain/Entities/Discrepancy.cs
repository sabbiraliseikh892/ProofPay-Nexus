using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Domain.Entities
{
    public class Discrepancy
    {
        public Guid Id { get; set; }

        public Guid PurchaseOrderId { get; set; }

        public Guid? EvidenceDocumentId { get; set; }

        public DiscrepancyType Type { get; set; }

        public DiscrepancySeverity Severity { get; set; }

        public DiscrepancyStatus Status { get; set; }
            = DiscrepancyStatus.Open;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? ExpectedValue { get; set; }

        public string? ActualValue { get; set; }

        public string? RecommendedAction { get; set; }

        public string? ResolutionNotes { get; set; }

        public bool IsAiDetected { get; set; }

        public DateTime DetectedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? ResolvedAtUtc { get; set; }

        // Navigation properties
        public PurchaseOrder? PurchaseOrder { get; set; }

        public EvidenceDocument? EvidenceDocument { get; set; }
    }
}
