using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Data.Repositories;
using BillPaymentProvider.Data;
using BillPaymentProvider.Infrastructure.Logging;
using BillPaymentProvider.Middleware;
using BillPaymentProvider.Providers;
using BillPaymentProvider.Services;
using Microsoft.EntityFrameworkCore;
using BillPaymentProvider.Configuration;
using BillPaymentProvider.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
            // Configuration des settings
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Section));
            services.Configure<SecuritySettings>(configuration.GetSection(SecuritySettings.Section));
            services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.Section));
            services.Configure<WebhookSettings>(configuration.GetSection(WebhookSettings.Section));
            services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.Section));
            services.Configure<LoggingSettings>(configuration.GetSection(LoggingSettings.Section));

            // Services principaux
            services.AddScoped<PaymentService>();
            services.AddScoped<TransactionService>();
            services.AddScoped<PaymentHistoryService>();
            services.AddScoped<BillerConfigService>();
            services.AddScoped<JwtService>();
            services.AddScoped<UserService>();

            // Repositories
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<BillerRepository>();

            // Providers
            services.AddScoped<GenericPaymentProvider>();

            // Utils et sécurité
            services.AddScoped<BruteForceProtection>();
            services.AddScoped<AuditLogger>();
            services.AddScoped<UserMigrationUtility>();

            // Services singleton
            services.AddSingleton<WebhookService>();

            // Middleware (seulement IdempotencyMiddleware qui implémente IMiddleware)
            services.AddScoped<IdempotencyMiddleware>();

            return services;
        }

        /// <summary>
        /// Configure l'authentification JWT
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection(JwtSettings.Section).Get<JwtSettings>() ?? new JwtSettings();

            if (string.IsNullOrEmpty(jwtSettings.Key) || jwtSettings.Key == "votre_cle_secrete_super_longue_a_personnaliser")
            {
                throw new InvalidOperationException("Une clé JWT sécurisée doit être configurée dans les paramètres");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("Échec d'authentification JWT: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var username = context.Principal?.Identity?.Name;
                        logger.LogDebug("Token JWT validé pour l'utilisateur: {Username}", username);
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        /// <summary>
        /// Configure le CORS pour l'application
        /// </summary>
        public static IServiceCollection AddApplicationCors(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", builder =>
                {
                    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                        ?? new[] { "http://localhost:5000", "http://localhost:5173", "http://localhost:5163" };

                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });

                options.AddPolicy("RestrictedCors", builder =>
                {
                    builder.WithOrigins(
                        "http://localhost:5000",
                        "http://localhost:5173",
                        "http://localhost:5163",
                        "http://localhost"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            return services;
        }

        /// <summary>
        /// Configure la limitation de débit (Rate Limiting) - Version simplifiée
        /// </summary>
        public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            // Pour le moment, nous désactivons le rate limiting car il nécessite .NET 7+
            // Vous pouvez implémenter une solution custom avec un middleware si nécessaire
            
            return services;
        }

        /// <summary>
        /// Configure la base de données
        /// </summary>
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseSettings = configuration.GetSection(DatabaseSettings.Section).Get<DatabaseSettings>() ?? new DatabaseSettings();
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? databaseSettings.ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Data Source=billpayment.db";
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                var sqliteOptions = options.UseSqlite(connectionString);
                
                if (databaseSettings.EnableSensitiveDataLogging)
                {
                    options.EnableSensitiveDataLogging();
                }
                
                if (databaseSettings.EnableDetailedErrors)
                {
                    options.EnableDetailedErrors();
                }

                if (databaseSettings.EnableServiceProviderCaching)
                {
                    options.EnableServiceProviderCaching();
                }
            });

            return services;
        }

        /// <summary>
        /// Configure la cache mémoire
        /// </summary>
        public static IServiceCollection AddApplicationCache(this IServiceCollection services, IConfiguration configuration)
        {
            var apiSettings = configuration.GetSection(ApiSettings.Section).Get<ApiSettings>() ?? new ApiSettings();

            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1000; // Limite du nombre d'entrées
                options.CompactionPercentage = 0.25; // Pourcentage à supprimer lors du nettoyage
            });

            return services;
        }

        /// <summary>
        /// Configure les services de santé (Health Checks)
        /// </summary>
        public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddCheck("database", () => 
                {
                    // Vérification simple de la base de données
                    try
                    {
                        using var scope = services.BuildServiceProvider().CreateScope();
                        var context = scope.ServiceProvider.GetService<AppDbContext>();
                        var canConnect = context?.Database.CanConnect() ?? false;
                        return canConnect ? 
                            Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy() : 
                            Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Impossible de se connecter à la base de données");
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy($"Erreur de base de données: {ex.Message}");
                    }
                })
                .AddCheck("webhook", () => 
                {
                    // Vérification simple du service webhook
                    var webhookSettings = configuration.GetSection(WebhookSettings.Section).Get<WebhookSettings>();
                    return webhookSettings?.Enabled == true ? 
                        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy() : 
                        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded("Webhook désactivé");
                });

            return services;
        }
    }
}
