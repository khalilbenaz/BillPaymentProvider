using Microsoft.EntityFrameworkCore;
using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Data;
using BillPaymentProvider.Extensions;
using BillPaymentProvider.Infrastructure.Logging;
using BillPaymentProvider.Middleware;
using BillPaymentProvider.Providers;
using BillPaymentProvider.Services;
using BillPaymentProvider.Utils;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Pour garder les noms de propriétés tels quels
});

builder.Services.AddEndpointsApiExplorer();

// Configurer Swagger
builder.Services.ConfigureSwagger();

// Configurer SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=billpayment.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Ajouter le service de logging personnalisé
builder.Services.AddSingleton<IAppLogger, SqliteAppLogger>();

// Configurer le middleware d'idempotence
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IdempotencyMiddleware>();

// Ajouter les services de l'application
builder.Services.AddApplicationServices(builder.Configuration);

// Configurer CORS
builder.Services.AddApplicationCors();

var app = builder.Build();

// Configurer le pipeline de requêtes HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Egypt BillPayment API v1");
    c.RoutePrefix = string.Empty;
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DefaultModelsExpandDepth(-1); // Cacher les modèles par défaut
});

// Utiliser les middlewares personnalisés
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

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
            Console.Error.WriteLine($"ERREUR: {ex.Message}");
        }
    }
}

app.Run();