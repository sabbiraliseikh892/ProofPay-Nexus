using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.AI
{
    public class InvoiceLineItemResult
    {
        public string? ItemName { get; set; }

        public string? SKU { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? UnitPrice { get; set; }

        public decimal? LineTotal { get; set; }
    }
}
