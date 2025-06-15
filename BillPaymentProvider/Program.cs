using Microsoft.EntityFrameworkCore;
using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Data;
using BillPaymentProvider.Extensions;
using BillPaymentProvider.Infrastructure.Logging;
using BillPaymentProvider.Middleware;
using BillPaymentProvider.Providers;
using BillPaymentProvider.Services;
using BillPaymentProvider.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using BillPaymentProvider.Validators;
using BillPaymentProvider.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog en premier
builder.ConfigureSerilog();

try
{
    Log.Information("Démarrage de l'application BillPaymentProvider");

    // Add services to the container
    builder.Services.AddControllers().AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Pour garder les noms de propriétés tels quels
    });

    builder.Services.AddEndpointsApiExplorer();

    // Configurer Swagger
    builder.Services.ConfigureSwagger();

    // Configurer la base de données
    builder.Services.AddDatabase(builder.Configuration);

    // Ajouter le service de logging personnalisé
    builder.Services.AddSingleton<IAppLogger, SqliteAppLogger>();

    // Configurer le cache mémoire et l'idempotence
    builder.Services.AddApplicationCache(builder.Configuration);

    // Ajouter les services de l'application
    builder.Services.AddApplicationServices(builder.Configuration);

    // Injection de AuditLogger et IHttpContextAccessor
    builder.Services.AddHttpContextAccessor();

    // Configurer CORS
    builder.Services.AddApplicationCors(builder.Configuration);

    // Configuration FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<B3gServiceRequestValidator>();

    // Configuration JWT avec les nouvelles extensions
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // Ajout de la limitation de débit
    builder.Services.AddRateLimiting(builder.Configuration);

    // Ajout des health checks
    builder.Services.AddApplicationHealthChecks(builder.Configuration);

    var app = builder.Build();

    // Configurer le pipeline de requêtes HTTP
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Egypt BillPayment API v1");
            c.RoutePrefix = string.Empty;
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            c.DefaultModelsExpandDepth(-1); // Cacher les modèles par défaut
        });
    }

    // Utiliser Serilog pour les logs de requêtes
    app.UseSerilogRequestLogging();

    // Utiliser les middlewares personnalisés dans l'ordre approprié
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();

    app.UseHttpsRedirection();
    
    app.UseCors("RestrictedCors");
    
    // Authentification et autorisation
    app.UseAuthentication();
    app.UseAuthorization();

    // Middleware d'idempotence après l'authentification
    app.UseMiddleware<IdempotencyMiddleware>();

    app.MapControllers();

    // Ajouter les endpoints de health check
    app.MapHealthChecks("/health");

    // Initialiser la base de données
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var logger = services.GetRequiredService<IAppLogger>();

            // S'assurer que la base de données est créée
            context.Database.EnsureCreated();

            // Initialiser les données - utiliser l'instance plutôt que la méthode statique
            if (!context.BillerConfigurations.Any())
            {
                logger.LogInfo("Initialisation de la base de données...");

                // Créer une instance de DataSeeder et appeler sa méthode SeedData
                var dataSeeder = new DataSeeder(context);
                dataSeeder.SeedData();

                logger.LogInfo("Base de données initialisée avec succès");
            }
            else
            {
                logger.LogInfo("Base de données déjà initialisée");
            }

            Log.Information("Application démarrée avec succès sur {Environment}", app.Environment.EnvironmentName);
        }
        catch (Exception ex)
        {
            try
            {
                var logger = services.GetRequiredService<IAppLogger>();
                logger.LogError($"Une erreur s'est produite lors de l'initialisation de la base de données: {ex.Message}");
            }
            catch
            {
                // Fallback si le logger n'est pas disponible
                Log.Fatal(ex, "Erreur fatale lors de l'initialisation de la base de données");
            }
            throw;
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "L'application a échoué au démarrage");
    throw;
}
finally
{
    Log.CloseAndFlush();
}