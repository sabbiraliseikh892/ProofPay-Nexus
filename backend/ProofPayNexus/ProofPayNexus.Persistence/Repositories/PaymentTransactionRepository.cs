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
    public class PaymentTransactionRepository
    : IPaymentTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentTransactionRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentTransaction?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);
        }

        public async Task<PaymentTransaction?> GetByPayPalOrderIdAsync(
            string payPalOrderId,
            CancellationToken cancellationToken = default)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(
                    x => x.PayPalOrderId == payPalOrderId,
                    cancellationToken);
        }

        public async Task AddAsync(
            PaymentTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            await _context.PaymentTransactions.AddAsync(
                transaction,
                cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
