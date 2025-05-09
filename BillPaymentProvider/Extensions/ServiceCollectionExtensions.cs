using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Data.Repositories;
using BillPaymentProvider.Data;
using BillPaymentProvider.Infrastructure.Logging;
using BillPaymentProvider.Middleware;
using BillPaymentProvider.Providers;
using BillPaymentProvider.Services;
using Microsoft.EntityFrameworkCore;

namespace BillPaymentProvider.Extensions
{
    /// <summary>
    /// Extensions pour la configuration des services de l'application
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure tous les services de l'application
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Services principaux
            services.AddScoped<PaymentService>();
            services.AddScoped<TransactionService>();
            services.AddScoped<PaymentHistoryService>();
            services.AddScoped<BillerConfigService>();

            // Repositories
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<BillerRepository>();

            // Providers
            services.AddScoped<GenericPaymentProvider>();

            return services;
        }

        /// <summary>
        /// Configure le CORS pour l'application
        /// </summary>
        public static IServiceCollection AddApplicationCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            return services;
        }
    }
}
