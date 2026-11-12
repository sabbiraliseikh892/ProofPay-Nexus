using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.PaymentAssessments
{
    public class PaymentAssessmentResponse
    {
        public Guid Id { get; set; }

        public Guid PurchaseOrderId { get; set; }

        public decimal RiskScore { get; set; }

        public string Recommendation { get; set; } = string.Empty;

        public string Explanation { get; set; } = string.Empty;

        public string? RiskFactorsJson { get; set; }

        public string ModelName { get; set; } = string.Empty;

        public DateTime AssessedAtUtc { get; set; }
    }
}
