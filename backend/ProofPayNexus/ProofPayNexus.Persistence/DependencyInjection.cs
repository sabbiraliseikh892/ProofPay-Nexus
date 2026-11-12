using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProofPayNexus.Application.Interfaces.Repositories;
using ProofPayNexus.Persistence.Context;
using ProofPayNexus.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence (this IServiceCollection services,IConfiguration configuration) 
        {

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")
                ));
            services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
            services.AddScoped<IEvidenceDocumentRepository, EvidenceDocumentRepository>();
            services.AddScoped<IEvidenceAnalysisRepository, EvidenceAnalysisRepository>();
            services.AddScoped<IDiscrepancyRepository, DiscrepancyRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IPaymentAssessmentRepository, PaymentAssessmentRepository>();
            services.AddScoped<
    IPaymentTransactionRepository,
    PaymentTransactionRepository>();
            return services;
        }
    }
}
