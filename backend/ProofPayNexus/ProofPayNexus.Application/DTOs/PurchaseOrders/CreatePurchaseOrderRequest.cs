using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.PurchaseOrders
{
    public class CreatePurchaseOrderRequest
    {
        [Required]
        [MaxLength(50)]
        public string PurchaseOrderNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string SupplierName { get; set; } = string.Empty;

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CurrencyCode { get; set; } = "USD";

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpectedDeliveryDate { get; set; }

        [Required]
        [MinLength(1)]
        public List<PurchaseOrderItemRequest> Items { get; set; } = new();
    }
}
