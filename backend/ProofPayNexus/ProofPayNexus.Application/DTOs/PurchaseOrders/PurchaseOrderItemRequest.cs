using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.PurchaseOrders
{
    public class PurchaseOrderItemRequest
    {
        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SKU { get; set; }

        [Range(0.001, 999999999999999.999)]
        public decimal Quantity { get; set; }

        [Range(0.01, 9999999999999999.99)]
        public decimal UnitPrice { get; set; }
    }
}
