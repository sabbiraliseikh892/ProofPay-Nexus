using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.Payments
{
    public class CreatePayPalOrderRequest
    {
        public Guid PurchaseOrderId { get; set; }
    }
}
