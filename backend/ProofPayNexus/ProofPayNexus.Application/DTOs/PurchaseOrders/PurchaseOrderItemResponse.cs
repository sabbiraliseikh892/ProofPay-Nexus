using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.PurchaseOrders
{
    public class PurchaseOrderItemResponse
    {
        public Guid Id { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string? SKU { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal { get; set; }
    }
}
