using ProofPayNexus.Application.DTOs.Discrepancies;
using ProofPayNexus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Services
{
    public interface IDiscrepancyService
    {
        Task<List<Discrepancy>> GetByPurchaseOrderIdAsync(
        Guid purchaseOrderId,
        CancellationToken cancellationToken = default);

        Task<Discrepancy?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Discrepancy> UpdateStatusAsync(
            Guid id,
            UpdateDiscrepancyStatusRequest request,
            string reviewedBy,
            CancellationToken cancellationToken = default);
    }
}
