using Microsoft.Extensions.DependencyInjection;
using ProofPayNexus.Application.Interfaces.AI;
using ProofPayNexus.Application.Interfaces.Services;
using ProofPayNexus.Application.Interfaces.Storage;
using ProofPayNexus.Infrastructure.AI;
using ProofPayNexus.Infrastructure.Payments;
using ProofPayNexus.Infrastructure.Storage;
using ProofPayNexus.Infrastructure.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
        {
            services.AddSingleton<
                IFileStorageService,
                LocalFileStorageService>();
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddHttpClient<IPayPalAuthService, PayPalAuthService>();
            services.AddHttpClient<
    IPayPalPaymentService,
    PayPalPaymentService>();
            services.AddHttpClient<IPayPalWebhookService, PayPalWebhookService>();

            return services;
        }
    }
}
