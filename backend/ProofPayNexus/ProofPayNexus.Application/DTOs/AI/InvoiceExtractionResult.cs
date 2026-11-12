using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.AI
{
    public class InvoiceExtractionResult
    {
        public string? InvoiceNumber { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public string? SupplierName { get; set; }

        public string? CurrencyCode { get; set; }

        public decimal? SubTotal { get; set; }

        public decimal? TaxAmount { get; set; }

        public decimal? TotalAmount { get; set; }

        public List<InvoiceLineItemResult> Items { get; set; } = new();

        public string? ExtractionNotes { get; set; }

        public decimal? ConfidenceScore { get; set; }
    }
}
