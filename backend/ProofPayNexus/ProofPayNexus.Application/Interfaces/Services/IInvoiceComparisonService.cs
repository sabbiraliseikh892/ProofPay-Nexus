using ProofPayNexus.Application.DTOs.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Services
{
    public interface IInvoiceComparisonService
    {
        Task<PurchaseOrderComparisonResult> CompareAsync(
       Guid evidenceDocumentId,
       CancellationToken cancellationToken = default);
    }
}
