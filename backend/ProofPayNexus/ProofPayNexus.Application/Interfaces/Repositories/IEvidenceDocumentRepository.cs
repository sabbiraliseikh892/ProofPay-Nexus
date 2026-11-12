using ProofPayNexus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Repositories
{
    public interface IEvidenceDocumentRepository
    {
        Task<EvidenceDocument?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

        Task<List<EvidenceDocument>> GetByPurchaseOrderIdAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            EvidenceDocument evidenceDocument,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
