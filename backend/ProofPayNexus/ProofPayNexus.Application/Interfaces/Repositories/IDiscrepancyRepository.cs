using ProofPayNexus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Repositories
{
    public interface IDiscrepancyRepository
    {
        Task<Discrepancy?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<Discrepancy>> GetByPurchaseOrderIdAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default);

        Task<List<Discrepancy>> GetByEvidenceDocumentIdAsync(
            Guid evidenceDocumentId,
            CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<Discrepancy> discrepancies,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }

}
