using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Domain.Enums
{
    public enum PurchaseOrderStatus
    {
        Draft = 1,
        Open = 2,
        PartiallyDelivered = 3,
        Completed = 4,
        Cancelled = 5
    }
}
