using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BillPaymentProvider.Core.Interfaces;

namespace BillPaymentProvider.Utils
{
    public class AuditLogger
    {
        private readonly IAppLogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogger(IAppLogger logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public void LogAction(string action, string details)
        {
            var user = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name
                ?? "anonymous";
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            _logger.LogInfo($"[AUDIT] {DateTime.UtcNow:u} | User: {user} | IP: {ip} | Action: {action} | Details: {details}");
        }
    }
}
