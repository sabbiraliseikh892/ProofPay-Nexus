using ProofPayNexus.Application.DTOs.PaymentAssessments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Services
{
    public interface IPaymentAssessmentService
    {
        Task<PaymentAssessmentResponse> AssessAsync(
       Guid purchaseOrderId,
       CancellationToken cancellationToken = default);

        Task<PaymentAssessmentResponse?> GetLatestByPurchaseOrderIdAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default);
    }
}
