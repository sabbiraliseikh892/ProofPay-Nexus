using ProofPayNexus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Repositories
{
    public interface IPurchaseOrderRepository
    {
        Task<PurchaseOrder?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

        Task<PurchaseOrder?> GetByPurchaseOrderNumberAsync(
            string purchaseOrderNumber,
            CancellationToken cancellationToken = default);

        Task<List<PurchaseOrder>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task AddAsync(
            PurchaseOrder purchaseOrder,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
