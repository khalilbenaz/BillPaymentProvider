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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Pour garder les noms de propri�t�s tels quels
});

builder.Services.AddEndpointsApiExplorer();

// Configurer Swagger
builder.Services.ConfigureSwagger();

// Configurer SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=billpayment.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Ajouter le service de logging personnalis�
builder.Services.AddSingleton<IAppLogger, SqliteAppLogger>();

// Configurer le middleware d'idempotence
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IdempotencyMiddleware>();

// Ajouter les services de l'application
builder.Services.AddApplicationServices(builder.Configuration);

// Injection du service UserService
builder.Services.AddScoped<UserService>();

// Configurer CORS
builder.Services.AddApplicationCors();

// Configuration JWT (clé à personnaliser)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "votre_cle_secrete_super_longue";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BillPaymentProvider";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

var app = builder.Build();

// Configurer le pipeline de requ�tes HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Egypt BillPayment API v1");
    c.RoutePrefix = string.Empty;
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DefaultModelsExpandDepth(-1); // Cacher les mod�les par d�faut
});

// Utiliser les middlewares personnalis�s
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication(); // Ajout de l'authentification
app.UseAuthorization();
app.MapControllers();

// Initialiser la base de donn�es
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<IAppLogger>();

        // S'assurer que la base de donn�es est cr��e
        context.Database.EnsureCreated();

        // Initialiser les donn�es - utiliser l'instance plut�t que la m�thode statique
        if (!context.BillerConfigurations.Any())
        {
            logger.LogInfo("Initialisation de la base de donn�es...");

            // Cr�er une instance de DataSeeder et appeler sa m�thode SeedData
            var dataSeeder = new DataSeeder(context);
            dataSeeder.SeedData();

            logger.LogInfo("Base de donn�es initialis�e avec succ�s");
        }
        else
        {
            logger.LogInfo("Base de donn�es d�j� initialis�e");
        }
    }
    catch (Exception ex)
    {
        try
        {
            var logger = services.GetRequiredService<IAppLogger>();
            logger.LogError($"Une erreur s'est produite lors de l'initialisation de la base de donn�es: {ex.Message}");
        }
        catch
        {
            // Fallback si le logger n'est pas disponible
            Console.Error.WriteLine($"ERREUR: {ex.Message}");
        }
    }
}

app.Run();