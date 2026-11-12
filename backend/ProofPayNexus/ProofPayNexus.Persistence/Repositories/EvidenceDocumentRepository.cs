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
    public class EvidenceDocumentRepository : IEvidenceDocumentRepository
    {
        private readonly ApplicationDbContext _context;
        public EvidenceDocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<EvidenceDocument?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        {
            return await _context.EvidenceDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public async Task<List<EvidenceDocument>> GetByPurchaseOrderIdAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default)
        {
            return await _context.EvidenceDocuments
                .AsNoTracking()
                .Where(x => x.PurchaseOrderId == purchaseOrderId)
                .OrderByDescending(x => x.UploadedAtUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            EvidenceDocument evidenceDocument,
            CancellationToken cancellationToken = default)
        {
            await _context.EvidenceDocuments.AddAsync(
                evidenceDocument,
                cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
