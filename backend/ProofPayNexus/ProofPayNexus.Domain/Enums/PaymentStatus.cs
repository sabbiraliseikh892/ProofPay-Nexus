using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 1,
        Created = 2,
        BuyerApproved = 3,
        Authorized = 4,
        CapturePending = 5,
        Captured = 6,
        Failed = 7,
        Voided = 8,
        Cancelled = 9
    }
}
