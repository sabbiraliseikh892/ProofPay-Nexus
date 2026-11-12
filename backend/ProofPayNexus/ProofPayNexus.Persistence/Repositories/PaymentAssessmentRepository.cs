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
    public class PaymentAssessmentRepository : IPaymentAssessmentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentAssessmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<PaymentAssessment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        {
            return await _context.PaymentAssessments
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public async Task<PaymentAssessment?> GetLatestByPurchaseOrderIdAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default)
        {
            return await _context.PaymentAssessments
                .AsNoTracking()
                .Where(x => x.PurchaseOrderId == purchaseOrderId)
                .OrderByDescending(x => x.AssessedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddAsync(
            PaymentAssessment assessment,
            CancellationToken cancellationToken = default)
        {
            await _context.PaymentAssessments.AddAsync(
                assessment,
                cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
