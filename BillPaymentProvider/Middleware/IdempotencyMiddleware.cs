using BillPaymentProvider.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text;
using BillPaymentProvider.Core.Interfaces;

namespace BillPaymentProvider.Middleware
{
    /// <summary>
    /// Middleware qui assure l'idempotence des requêtes de paiement
    /// </summary>
    public class IdempotencyMiddleware : IMiddleware
    {
        private readonly IMemoryCache _cache;
        private readonly IAppLogger _logger;

        // Durée de vie du cache (24 heures)
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);

        public IdempotencyMiddleware(IMemoryCache cache, IAppLogger logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Vérifier si c'est une requête de paiement
            if (IsPaymentRequest(context.Request))
            {
                // Capturer le corps de la requête pour le lire
                context.Request.EnableBuffering();

                // Lire le corps de la requête
                string requestBody;
                using (var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    // Remettre le stream à zéro pour que les autres middlewares puissent le lire
                    context.Request.Body.Position = 0;
                }

                try
                {
                    // Désérialiser pour obtenir le SessionId
                    var request = JsonSerializer.Deserialize<B3gServiceRequest>(requestBody);

                    if (request != null && !string.IsNullOrEmpty(request.SessionId))
                    {
                        string idempotencyKey = request.SessionId;

                        // Vérifier si la requête existe déjà en cache
                        if (_cache.TryGetValue(idempotencyKey, out var cachedResponse))
                        {
                            _logger.LogInfo($"Requête dupliquée détectée: {idempotencyKey}, retournant réponse en cache");

                            // Retourner la réponse mise en cache
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = 200;
                            await context.Response.WriteAsync(cachedResponse.ToString() ?? "");

                            return;
                        }

                        // Capturer la réponse pour la mettre en cache
                        var originalBodyStream = context.Response.Body;
                        using var responseBody = new MemoryStream();
                        context.Response.Body = responseBody;

                        // Continuer l'exécution du pipeline
                        await next(context);

                        // Lire la réponse générée
                        responseBody.Seek(0, SeekOrigin.Begin);
                        var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();

                        // Mettre en cache la réponse
                        _cache.Set(idempotencyKey, responseBodyText, _cacheDuration);

                        // Copier la réponse au stream de réponse original
                        responseBody.Seek(0, SeekOrigin.Begin);
                        await responseBody.CopyToAsync(originalBodyStream);

                        context.Response.Body = originalBodyStream;
                        return;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"Erreur lors de la désérialisation de la requête: {ex.Message}");
                }
            }

            // Si ce n'est pas une requête de paiement ou s'il y a eu une erreur, continuer normalement
            await next(context);
        }

        private bool IsPaymentRequest(HttpRequest request)
        {
            return request.Method == "POST" &&
                   request.Path.Value != null &&
                   request.Path.Value.Contains("/api/Payment/process", StringComparison.OrdinalIgnoreCase);
        }
    }
}
