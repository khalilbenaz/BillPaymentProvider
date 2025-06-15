using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Data;

namespace BillPaymentProvider.Infrastructure.Logging
{
    /// <summary>
    /// Logger qui enregistre les logs dans SQLite
    /// </summary>
    public class SqliteAppLogger : IAppLogger
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SqliteAppLogger(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void LogInfo(string message)
        {
            Log(LogLevel.Information, message);
        }

        public void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public async Task LogInfoAsync(string message)
        {
            await LogAsync(LogLevel.Information, message);
        }

        public async Task LogWarningAsync(string message)
        {
            await LogAsync(LogLevel.Warning, message);
        }

        public async Task LogErrorAsync(string message)
        {
            await LogAsync(LogLevel.Error, message);
        }

        public async Task LogDebugAsync(string message)
        {
            await LogAsync(LogLevel.Debug, message);
        }

        private void Log(LogLevel level, string message)
        {
            // Utilisation de Task.Run pour éviter de bloquer le thread appelant
            Task.Run(async () => await LogAsync(level, message)).ConfigureAwait(false);
        }

        private async Task LogAsync(LogLevel level, string message)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var logEntry = new LogEntry
            {
                Message = message,
                Level = level,
                Timestamp = DateTime.UtcNow
            };

            dbContext.LogEntries.Add(logEntry);
            await dbContext.SaveChangesAsync();
        }

      
    }
}
