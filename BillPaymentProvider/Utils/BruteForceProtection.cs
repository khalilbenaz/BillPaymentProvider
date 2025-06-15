using System.Collections.Concurrent;
using BillPaymentProvider.Configuration;
using Microsoft.Extensions.Options;

namespace BillPaymentProvider.Utils
{
    /// <summary>
    /// Service pour la protection contre les attaques par force brute
    /// </summary>
    public class BruteForceProtection
    {
        private readonly ConcurrentDictionary<string, (int Attempts, DateTime? LockoutUntil)> _loginAttempts = new();
        private readonly SecuritySettings _securitySettings;
        private readonly ILogger<BruteForceProtection> _logger;

        public BruteForceProtection(IOptions<SecuritySettings> securitySettings, ILogger<BruteForceProtection> logger)
        {
            _securitySettings = securitySettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Vérifie si un utilisateur est temporairement bloqué
        /// </summary>
        public bool IsLockedOut(string username)
        {
            if (!_securitySettings.EnableBruteForceProtection)
                return false;

            if (_loginAttempts.TryGetValue(username, out var entry))
            {
                if (entry.LockoutUntil.HasValue && entry.LockoutUntil.Value > DateTime.UtcNow)
                {
                    _logger.LogWarning("Tentative de connexion sur compte bloqué: {Username}", username);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Enregistre une tentative de connexion échouée
        /// </summary>
        public void RegisterFailedAttempt(string username, string? ipAddress = null)
        {
            if (!_securitySettings.EnableBruteForceProtection)
                return;

            _loginAttempts.AddOrUpdate(username,
                (1, null),
                (key, old) =>
                {
                    int attempts = old.Attempts + 1;
                    DateTime? lockoutUntil = null;
                    
                    if (attempts >= _securitySettings.MaxLoginAttempts)
                    {
                        lockoutUntil = DateTime.UtcNow.AddMinutes(_securitySettings.LockoutDurationMinutes);
                        _logger.LogWarning("Compte {Username} bloqué après {Attempts} tentatives échouées. IP: {IpAddress}", 
                            username, attempts, ipAddress);
                    }
                    else
                    {
                        _logger.LogInformation("Tentative de connexion échouée {Attempts}/{MaxAttempts} pour {Username}. IP: {IpAddress}", 
                            attempts, _securitySettings.MaxLoginAttempts, username, ipAddress);
                    }
                    
                    return (attempts, lockoutUntil);
                });
        }

        /// <summary>
        /// Remet à zéro les tentatives pour un utilisateur après une connexion réussie
        /// </summary>
        public void ResetAttempts(string username)
        {
            if (_loginAttempts.TryRemove(username, out var removed) && removed.Attempts > 0)
            {
                _logger.LogInformation("Tentatives de connexion remises à zéro pour {Username}", username);
            }
        }

        /// <summary>
        /// Obtient le nombre de tentatives restantes avant le blocage
        /// </summary>
        public int GetRemainingAttempts(string username)
        {
            if (!_securitySettings.EnableBruteForceProtection)
                return int.MaxValue;

            if (_loginAttempts.TryGetValue(username, out var entry))
            {
                return Math.Max(0, _securitySettings.MaxLoginAttempts - entry.Attempts);
            }
            return _securitySettings.MaxLoginAttempts;
        }

        /// <summary>
        /// Obtient le temps restant avant la fin du blocage
        /// </summary>
        public TimeSpan? GetLockoutTimeRemaining(string username)
        {
            if (_loginAttempts.TryGetValue(username, out var entry) && entry.LockoutUntil.HasValue)
            {
                var remaining = entry.LockoutUntil.Value - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
            return null;
        }

        /// <summary>
        /// Nettoie les entrées expirées (à appeler périodiquement)
        /// </summary>
        public void CleanupExpiredEntries()
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _loginAttempts
                .Where(kvp => kvp.Value.LockoutUntil.HasValue && kvp.Value.LockoutUntil.Value <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _loginAttempts.TryRemove(key, out _);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogDebug("Nettoyage de {Count} entrées expirées du cache brute force", expiredKeys.Count);
            }
        }
    }
}
