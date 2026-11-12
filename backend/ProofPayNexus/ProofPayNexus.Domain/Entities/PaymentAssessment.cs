using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Domain.Entities
{
    public class PaymentAssessment
    {
        public Guid Id { get; set; }

        public Guid PurchaseOrderId { get; set; }

        public decimal RiskScore { get; set; }

        public PaymentRecommendation Recommendation { get; set; }

        public string Explanation { get; set; } = string.Empty;

        public string? RiskFactorsJson { get; set; }

        public string ModelName { get; set; } = string.Empty;

        public DateTime AssessedAtUtc { get; set; }
            = DateTime.UtcNow;

        // Navigation properties
        public PurchaseOrder? PurchaseOrder { get; set; }

        public ICollection<PaymentTransaction> PaymentTransactions { get; set; }
            = new List<PaymentTransaction>();
    }
}
