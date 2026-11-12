using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Domain.Enums
{
    public enum DiscrepancyType
    {
        QuantityMismatch = 1,
        PriceMismatch = 2,
        MissingItems = 3,
        DamagedItems = 4,
        InvoiceMismatch = 5,
        MissingEvidence = 6,
        Other = 7
    }
}
