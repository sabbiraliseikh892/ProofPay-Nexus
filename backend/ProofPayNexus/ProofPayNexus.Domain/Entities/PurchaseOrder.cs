using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Domain.Entities
{
    public class PurchaseOrder
    {
        public Guid Id { get; set; }

        public string PurchaseOrderNumber { get; set; } = string.Empty;

        public string SupplierName { get; set; } = string.Empty;

        public string CurrencyCode { get; set; } = "USD";

        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public PurchaseOrderStatus Status { get; set; }
            = PurchaseOrderStatus.Draft;

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? UpdatedAtUtc { get; set; }

        // Navigation property
        public ICollection<PurchaseOrderItem> Items { get; set; }
            = new List<PurchaseOrderItem>();

        public ICollection<Discrepancy> Discrepancies { get; set; }
            = new List<Discrepancy>();
        public ICollection<PaymentAssessment> PaymentAssessments { get; set; }
            = new List<PaymentAssessment>();

        public ICollection<PaymentTransaction> PaymentTransactions { get; set; }
            = new List<PaymentTransaction>();
    }
}
