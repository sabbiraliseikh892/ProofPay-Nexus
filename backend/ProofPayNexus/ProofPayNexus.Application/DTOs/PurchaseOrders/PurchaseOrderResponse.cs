using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.PurchaseOrders
{
    public class PurchaseOrderResponse
    {
        public Guid Id { get; set; }

        public string PurchaseOrderNumber { get; set; } = string.Empty;

        public string SupplierName { get; set; } = string.Empty;

        public string CurrencyCode { get; set; } = "USD";

        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public PurchaseOrderStatus Status { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public List<PurchaseOrderItemResponse> Items { get; set; } = new();
    }
}
