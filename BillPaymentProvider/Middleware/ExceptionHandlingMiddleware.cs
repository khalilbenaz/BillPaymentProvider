using BillPaymentProvider.Core.Interfaces;
using System.Net;
using System.Text.Json;

namespace BillPaymentProvider.Middleware
{
    /// <summary>
    /// Middleware pour la gestion globale des exceptions
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAppLogger _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            IAppLogger logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Une erreur non gérée s'est produite: {ex}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            object response;

            if (_env.IsDevelopment())
            {
                response = new
                {
                    Status = "Error",
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    InnerException = exception.InnerException?.Message
                };
            }
            else
            {
                response = new
                {
                    Status = "Error",
                    Message = "Une erreur interne s'est produite. Veuillez réessayer ultérieurement."
                };
            }

            var jsonResponse = JsonSerializer.Serialize(response);

            return context.Response.WriteAsync(jsonResponse);
        }
    }
}
