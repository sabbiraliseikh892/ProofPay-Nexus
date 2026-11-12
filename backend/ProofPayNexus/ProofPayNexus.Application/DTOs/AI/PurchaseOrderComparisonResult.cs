using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.AI
{
    public class PurchaseOrderComparisonResult
    {
        public Guid PurchaseOrderId { get; set; }

        public Guid EvidenceDocumentId { get; set; }

        public string? PurchaseOrderNumber { get; set; }

        public string? InvoiceNumber { get; set; }

        public string? SupplierName { get; set; }

        public decimal PurchaseOrderTotal { get; set; }

        public decimal? InvoiceTotal { get; set; }

        public decimal? TotalDifference { get; set; }

        public bool IsSupplierMatching { get; set; }

        public bool IsTotalMatching { get; set; }

        public int TotalItemsCompared { get; set; }

        public int MatchingItems { get; set; }

        public int DiscrepantItems { get; set; }

        public List<InvoiceItemComparisonResult> Items { get; set; } = new();

        public List<string> Findings { get; set; } = new();

        public bool RequiresReview { get; set; }
    }
}
