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
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseOrder?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public async Task<PurchaseOrder?> GetByPurchaseOrderNumberAsync(
            string purchaseOrderNumber,
            CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders
                .FirstOrDefaultAsync(
                    x => x.PurchaseOrderNumber == purchaseOrderNumber,
                    cancellationToken);
        }

        public async Task<List<PurchaseOrder>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.PurchaseOrders
                .AsNoTracking()
                .Include(x => x.Items)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(
            PurchaseOrder purchaseOrder,
            CancellationToken cancellationToken = default)
        {
            await _context.PurchaseOrders.AddAsync(
                purchaseOrder,
                cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
