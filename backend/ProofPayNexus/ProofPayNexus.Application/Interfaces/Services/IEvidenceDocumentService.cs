using ProofPayNexus.Application.DTOs.Evidence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Services
{
    public interface IEvidenceDocumentService
    {
        Task<EvidenceDocumentResponse> UploadAsync(
            UploadEvidenceRequest request,
            CancellationToken cancellationToken = default);

        Task<List<EvidenceDocumentResponse>> GetByPurchaseOrderIdAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default);

        Task<EvidenceDocumentResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
