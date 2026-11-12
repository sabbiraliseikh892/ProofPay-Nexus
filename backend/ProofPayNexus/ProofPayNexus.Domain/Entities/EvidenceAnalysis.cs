using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Domain.Entities
{
    public class EvidenceAnalysis
    {
        public Guid Id { get; set; }

        public Guid EvidenceDocumentId { get; set; }

        public string ModelName { get; set; } = string.Empty;

        public string? ExtractedDataJson { get; set; }

        public string? AnalysisSummary { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public EvidenceAnalysisStatus Status { get; set; }
            = EvidenceAnalysisStatus.Pending;

        public string? ErrorMessage { get; set; }

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? CompletedAtUtc { get; set; }

        // Navigation property
        public EvidenceDocument? EvidenceDocument { get; set; }
    }
}
