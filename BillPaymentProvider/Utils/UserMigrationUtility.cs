using Microsoft.EntityFrameworkCore;
using BillPaymentProvider.Data;
using BillPaymentProvider.Core.Models;

namespace BillPaymentProvider.Utils
{
    /// <summary>
    /// Utilitaire pour migrer les utilisateurs existants de SHA256 vers BCrypt
    /// </summary>
    public class UserMigrationUtility
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserMigrationUtility> _logger;

        public UserMigrationUtility(AppDbContext context, ILogger<UserMigrationUtility> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Migre tous les utilisateurs existants vers BCrypt avec des mots de passe par défaut
        /// </summary>
        public async Task<bool> MigrateUsersTooBCryptAsync()
        {
            try
            {
                _logger.LogInformation("Début de la migration des utilisateurs vers BCrypt");

                var users = await _context.Users.ToListAsync();
                
                if (!users.Any())
                {
                    _logger.LogWarning("Aucun utilisateur trouvé pour la migration");
                    return true;
                }

                var userPasswordMapping = new Dictionary<string, string>
                {
                    { "admin", "Admin123!" },
                    { "user", "User123!" },
                    { "manager", "Manager123!" }
                };

                int migratedCount = 0;

                foreach (var user in users)
                {
                    if (userPasswordMapping.TryGetValue(user.Username.ToLower(), out var defaultPassword))
                    {
                        // Vérifier si le mot de passe est déjà en BCrypt (commence par $2)
                        if (!user.PasswordHash.StartsWith("$2"))
                        {
                            var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
                            user.PasswordHash = newHashedPassword;
                            migratedCount++;
                            
                            _logger.LogInformation("Utilisateur {Username} migré vers BCrypt", user.Username);
                        }
                        else
                        {
                            _logger.LogInformation("Utilisateur {Username} déjà en BCrypt, ignoré", user.Username);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Aucun mot de passe par défaut trouvé pour l'utilisateur {Username}", user.Username);
                    }
                }

                if (migratedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Migration terminée avec succès. {Count} utilisateurs migrés", migratedCount);
                }
                else
                {
                    _logger.LogInformation("Aucun utilisateur n'avait besoin d'être migré");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la migration des utilisateurs vers BCrypt");
                return false;
            }
        }

        /// <summary>
        /// Affiche les informations des utilisateurs actuels
        /// </summary>
        public async Task DisplayUsersInfoAsync()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                
                _logger.LogInformation("=== Informations des utilisateurs ===");
                foreach (var user in users)
                {
                    var hashType = user.PasswordHash.StartsWith("$2") ? "BCrypt" : "SHA256";
                    _logger.LogInformation("Utilisateur: {Username}, Rôle: {Role}, Hash: {HashType}", 
                        user.Username, user.Role, hashType);
                }
                _logger.LogInformation("=== Fin des informations ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'affichage des informations utilisateurs");
            }
        }
    }
}
