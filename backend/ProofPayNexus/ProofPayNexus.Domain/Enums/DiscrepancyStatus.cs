using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Domain.Enums
{
    public enum DiscrepancyStatus
    {
        Open = 1,
        UnderReview = 2,
        Resolved = 3,
        Accepted = 4,
        Rejected = 5
    }
}
