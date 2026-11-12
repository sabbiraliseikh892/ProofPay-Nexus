using Microsoft.EntityFrameworkCore;
using ProofPayNexus.Application.Interfaces.Repositories;
using ProofPayNexus.Domain.Entities;
using ProofPayNexus.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Persistence.Repositories
{
    public class EvidenceAnalysisRepository : IEvidenceAnalysisRepository
    {
        private readonly ApplicationDbContext _context;
        public EvidenceAnalysisRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<EvidenceAnalysis?> GetByIdAsync(
         Guid id,
         CancellationToken cancellationToken = default)
        {
            return await _context.EvidenceAnalyses
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }
        public async Task<EvidenceAnalysis?>
        GetLatestByEvidenceDocumentIdAsync(
            Guid evidenceDocumentId,
            CancellationToken cancellationToken = default)
        {
            return await _context.EvidenceAnalyses
                .AsNoTracking()
                .Where(x => x.EvidenceDocumentId == evidenceDocumentId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);
        }
        public async Task AddAsync(
    EvidenceAnalysis analysis,
    CancellationToken cancellationToken = default)
        {
            await _context.EvidenceAnalyses.AddAsync(
                analysis,
                cancellationToken);
        }
        public async Task SaveChangesAsync(
    CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}
