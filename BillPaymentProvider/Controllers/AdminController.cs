using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillPaymentProvider.Utils;

namespace BillPaymentProvider.Controllers
{
    /// <summary>
    /// Contrôleur d'administration pour les tâches de maintenance
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserMigrationUtility _migrationUtility;
        private readonly ILogger<AdminController> _logger;

        public AdminController(UserMigrationUtility migrationUtility, ILogger<AdminController> logger)
        {
            _migrationUtility = migrationUtility;
            _logger = logger;
        }

        /// <summary>
        /// Migre les utilisateurs existants de SHA256 vers BCrypt
        /// </summary>
        [HttpPost("migrate-users")]
        public async Task<IActionResult> MigrateUsers()
        {
            try
            {
                _logger.LogInformation("Demande de migration des utilisateurs reçue");
                
                var success = await _migrationUtility.MigrateUsersTooBCryptAsync();
                
                if (success)
                {
                    return Ok(new { 
                        Message = "Migration des utilisateurs terminée avec succès",
                        Timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(500, new { 
                        Message = "Erreur lors de la migration des utilisateurs",
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la migration des utilisateurs");
                return StatusCode(500, new { 
                    Message = "Erreur interne du serveur",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Affiche les informations des utilisateurs actuels
        /// </summary>
        [HttpGet("users-info")]
        public async Task<IActionResult> GetUsersInfo()
        {
            try
            {
                await _migrationUtility.DisplayUsersInfoAsync();
                return Ok(new { 
                    Message = "Informations des utilisateurs affichées dans les logs",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'affichage des informations utilisateurs");
                return StatusCode(500, new { 
                    Message = "Erreur interne du serveur",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Point de santé pour l'administration
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                Status = "Healthy",
                Service = "Admin Controller",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
