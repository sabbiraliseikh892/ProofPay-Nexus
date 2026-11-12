using ProofPayNexus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Repositories
{
    public interface IPaymentAssessmentRepository
    {
        Task<PaymentAssessment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

        Task<PaymentAssessment?> GetLatestByPurchaseOrderIdAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            PaymentAssessment assessment,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
