using ProofPayNexus.Application.DTOs.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Services
{
    public interface IPayPalPaymentService
    {
        Task<PayPalOrderResponse> CreateAuthorizationOrderAsync(
      Guid purchaseOrderId,
      CancellationToken cancellationToken = default);

        Task<PayPalAuthorizationResponse> AuthorizeOrderAsync(
            string payPalOrderId,
            CancellationToken cancellationToken = default);

        Task<PayPalCaptureResponse> CaptureAuthorizedPaymentAsync(
            Guid paymentTransactionId,
            CancellationToken cancellationToken = default);

    }
}
