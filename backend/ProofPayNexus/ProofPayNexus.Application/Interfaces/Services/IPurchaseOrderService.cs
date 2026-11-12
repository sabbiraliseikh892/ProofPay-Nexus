using ProofPayNexus.Application.DTOs.PurchaseOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Services
{
    public interface IPurchaseOrderService
    {
        Task<PurchaseOrderResponse> CreateAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default);

        Task<PurchaseOrderResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<PurchaseOrderResponse>> GetAllAsync(
            CancellationToken cancellationToken = default);
    }
}
