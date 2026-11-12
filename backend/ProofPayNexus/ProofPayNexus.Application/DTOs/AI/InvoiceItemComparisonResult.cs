using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.AI
{
    public class InvoiceItemComparisonResult
    {
        public string? PurchaseOrderItemName { get; set; }

        public string? InvoiceItemName { get; set; }

        public string? SKU { get; set; }

        public decimal? PurchaseOrderQuantity { get; set; }

        public decimal? InvoiceQuantity { get; set; }

        public decimal? PurchaseOrderUnitPrice { get; set; }

        public decimal? InvoiceUnitPrice { get; set; }

        public decimal? PurchaseOrderLineTotal { get; set; }

        public decimal? InvoiceLineTotal { get; set; }

        public bool IsMatching { get; set; }

        public List<string> Discrepancies { get; set; } = new();
    }
}
