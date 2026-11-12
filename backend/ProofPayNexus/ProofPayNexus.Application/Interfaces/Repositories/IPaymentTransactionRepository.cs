using ProofPayNexus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Repositories
{
    public interface IPaymentTransactionRepository
    {
        Task<PaymentTransaction?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<PaymentTransaction?> GetByPayPalOrderIdAsync(
            string payPalOrderId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            PaymentTransaction transaction,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
