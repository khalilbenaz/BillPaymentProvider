using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Data;
using Microsoft.EntityFrameworkCore;

namespace BillPaymentProvider.Services
{
    /// <summary>
    /// Service pour la gestion des logs système
    /// </summary>
    public class LoggingService
    {
        private readonly IAppLogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public LoggingService(IAppLogger logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Récupère les logs système
        /// </summary>
        public async Task<List<LogEntry>> GetLogsAsync(
            LogLevel? level = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int skip = 0,
            int take = 100)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                IQueryable<LogEntry> query = dbContext.LogEntries;

                // Filtrer par niveau de log
                if (level.HasValue)
                {
                    query = query.Where(l => l.Level == level.Value);
                }

                // Filtrer par date de début
                if (startDate.HasValue)
                {
                    var start = startDate.Value.Date;
                    query = query.Where(l => l.Timestamp >= start);
                }

                // Filtrer par date de fin
                if (endDate.HasValue)
                {
                    var end = endDate.Value.Date.AddDays(1).AddTicks(-1); // Fin de journée
                    query = query.Where(l => l.Timestamp <= end);
                }

                // Pagination
                query = query.OrderByDescending(l => l.Timestamp)
                              .Skip(skip)
                              .Take(take);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération des logs: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère les logs de niveau Error
        /// </summary>
        public async Task<List<LogEntry>> GetErrorLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int skip = 0,
            int take = 100)
        {
            return await GetLogsAsync(LogLevel.Error, startDate, endDate, skip, take);
        }

        /// <summary>
        /// Récupère le nombre de logs par niveau
        /// </summary>
        public async Task<Dictionary<LogLevel, int>> GetLogCountByLevelAsync(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                IQueryable<LogEntry> query = dbContext.LogEntries;

                // Filtrer par date de début
                if (startDate.HasValue)
                {
                    var start = startDate.Value.Date;
                    query = query.Where(l => l.Timestamp >= start);
                }

                // Filtrer par date de fin
                if (endDate.HasValue)
                {
                    var end = endDate.Value.Date.AddDays(1).AddTicks(-1); // Fin de journée
                    query = query.Where(l => l.Timestamp <= end);
                }

                // Compter par niveau
                var result = await query.GroupBy(l => l.Level)
                                        .Select(g => new { Level = g.Key, Count = g.Count() })
                                        .ToListAsync();

                // Convertir en dictionnaire
                var counts = new Dictionary<LogLevel, int>();
                foreach (var item in result)
                {
                    counts[item.Level] = item.Count;
                }

                // S'assurer que tous les niveaux sont présents
                foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
                {
                    if (!counts.ContainsKey(level))
                    {
                        counts[level] = 0;
                    }
                }

                return counts;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération du nombre de logs: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Purge les logs anciens
        /// </summary>
        public async Task PurgeOldLogsAsync(int daysToKeep = 30)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

                // Supprimer les logs plus anciens que la date limite
                var oldLogs = await dbContext.LogEntries
                                            .Where(l => l.Timestamp < cutoffDate)
                                            .ToListAsync();

                if (oldLogs.Any())
                {
                    dbContext.LogEntries.RemoveRange(oldLogs);
                    await dbContext.SaveChangesAsync();

                    _logger.LogInfo($"Purge de {oldLogs.Count} logs antérieurs à {cutoffDate}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la purge des logs: {ex.Message}");
                throw;
            }
        }
    }
}
