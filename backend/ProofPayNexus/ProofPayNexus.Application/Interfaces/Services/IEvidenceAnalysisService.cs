using ProofPayNexus.Application.DTOs.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Services
{
    public interface IEvidenceAnalysisService
    {
        Task<InvoiceExtractionResult> AnalyzeEvidenceAsync(
       Guid evidenceDocumentId,
       bool forceReanalysis = false,
       CancellationToken cancellationToken = default);
    }
}
