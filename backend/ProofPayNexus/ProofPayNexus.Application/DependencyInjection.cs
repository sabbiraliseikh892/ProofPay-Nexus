using Microsoft.Extensions.DependencyInjection;
using ProofPayNexus.Application.Interfaces.Services;
using ProofPayNexus.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
        this IServiceCollection services)
        {
            services.AddScoped<
                IPurchaseOrderService,
                PurchaseOrderService>();
            services.AddScoped<
         IEvidenceDocumentService,
         EvidenceDocumentService>();
            services.AddScoped<IEvidenceAnalysisService, EvidenceAnalysisService>();
            services.AddScoped<IInvoiceComparisonService, InvoiceComparisonService>();
            services.AddScoped<IDiscrepancyService, DiscrepancyService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<PaymentRiskEngine>();
            services.AddScoped<IPaymentAssessmentService, PaymentAssessmentService>();

            return services;
        }
    }
}
