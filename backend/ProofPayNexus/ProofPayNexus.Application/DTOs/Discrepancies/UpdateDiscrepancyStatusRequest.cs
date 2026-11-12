using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.Discrepancies
{
    public class UpdateDiscrepancyStatusRequest
    {
        public DiscrepancyStatus Status { get; set; }

        public string? ResolutionNotes { get; set; }
    }
}
