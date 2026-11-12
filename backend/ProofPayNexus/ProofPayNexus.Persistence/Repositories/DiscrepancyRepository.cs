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
    public class DiscrepancyRepository : IDiscrepancyRepository
    {
        private readonly ApplicationDbContext _context;

        public DiscrepancyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieve one discrepancy.
        // Tracking is enabled because this entity will be updated.
        public async Task<Discrepancy?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Discrepancies
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        // Retrieve all discrepancies associated with a purchase order.
        public async Task<List<Discrepancy>> GetByPurchaseOrderIdAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Discrepancies
                .AsNoTracking()
                .Where(x => x.PurchaseOrderId == purchaseOrderId)
                .OrderByDescending(x => x.DetectedAtUtc)
                .ToListAsync(cancellationToken);
        }

        // Retrieve discrepancies associated with an evidence document.
        public async Task<List<Discrepancy>> GetByEvidenceDocumentIdAsync(
            Guid evidenceDocumentId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Discrepancies
                .AsNoTracking()
                .Where(x => x.EvidenceDocumentId == evidenceDocumentId)
                .OrderByDescending(x => x.DetectedAtUtc)
                .ToListAsync(cancellationToken);
        }

        // Add multiple discrepancies.
        public async Task AddRangeAsync(
            IEnumerable<Discrepancy> discrepancies,
            CancellationToken cancellationToken = default)
        {
            await _context.Discrepancies.AddRangeAsync(
                discrepancies,
                cancellationToken);
        }

        // Save database changes.
        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
