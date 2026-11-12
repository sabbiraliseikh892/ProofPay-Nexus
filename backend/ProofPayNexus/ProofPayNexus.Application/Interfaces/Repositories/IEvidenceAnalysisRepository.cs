using ProofPayNexus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Repositories
{
    public interface IEvidenceAnalysisRepository
    {
        Task<EvidenceAnalysis?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default);

        Task<EvidenceAnalysis?> GetLatestByEvidenceDocumentIdAsync(
            Guid evidenceDocumentId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            EvidenceAnalysis analysis,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
