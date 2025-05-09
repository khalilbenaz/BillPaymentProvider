using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BillPaymentProvider.Utils;

namespace BillPaymentProvider.Controllers
{
    /// <summary>
    /// Contrôleur d'administration pour la gestion des créanciers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class BillerConfigController : ControllerBase
    {
        private readonly BillerRepository _billerRepository;
        private readonly AuditLogger _auditLogger;

        public BillerConfigController(BillerRepository billerRepository, AuditLogger auditLogger)
        {
            _billerRepository = billerRepository;
            _auditLogger = auditLogger;
        }

        /// <summary>
        /// Récupère tous les créanciers configurés
        /// </summary>
        /// <returns>Liste des créanciers</returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,User")]
        [ProducesResponseType(typeof(List<BillerConfiguration>), 200)]
        public async Task<IActionResult> GetAllBillers()
        {
            _auditLogger.LogAction("Liste créanciers", "");
            var billers = await _billerRepository.GetAllAsync();
            return Ok(billers);
        }

        /// <summary>
        /// Récupère un créancier par son code
        /// </summary>
        /// <param name="code">Code unique du créancier</param>
        /// <returns>Configuration du créancier</returns>
        [HttpGet("{code}")]
        [Authorize(Roles = "Admin,Manager,User")]
        [ProducesResponseType(typeof(BillerConfiguration), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBillerByCode(string code)
        {
            _auditLogger.LogAction("Détail créancier", $"BillerCode={code}");
            var biller = await _billerRepository.GetByCodeAsync(code);

            if (biller == null)
            {
                return NotFound();
            }

            return Ok(biller);
        }

        /// <summary>
        /// Crée un nouveau créancier
        /// </summary>
        /// <param name="biller">Configuration du créancier</param>
        /// <returns>Créancier créé</returns>
        [HttpPost]
        [ProducesResponseType(typeof(BillerConfiguration), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateBiller([FromBody] BillerConfiguration biller)
        {
            if (biller == null)
            {
                return BadRequest("Configuration invalide");
            }

            // Vérifier si le code est unique
            var existing = await _billerRepository.GetByCodeAsync(biller.BillerCode);
            if (existing != null)
            {
                return BadRequest($"Un créancier avec le code '{biller.BillerCode}' existe déjà");
            }

            var createdBiller = await _billerRepository.CreateAsync(biller);

            return CreatedAtAction(
                nameof(GetBillerByCode),
                new { code = createdBiller.BillerCode },
                createdBiller
            );
        }

        /// <summary>
        /// Met à jour un créancier existant
        /// </summary>
        /// <param name="code">Code unique du créancier</param>
        /// <param name="biller">Nouvelle configuration</param>
        /// <returns>Créancier mis à jour</returns>
        [HttpPut("{code}")]
        [ProducesResponseType(typeof(BillerConfiguration), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateBiller(string code, [FromBody] BillerConfiguration biller)
        {
            if (biller == null)
            {
                return BadRequest("Configuration invalide");
            }

            // Vérifier si le créancier existe
            var existing = await _billerRepository.GetByCodeAsync(code);
            if (existing == null)
            {
                return NotFound();
            }

            // Mettre à jour l'ID et le code
            biller.Id = existing.Id;
            biller.BillerCode = code; // S'assurer que le code ne change pas
            biller.UpdatedAt = DateTime.UtcNow;

            var updatedBiller = await _billerRepository.UpdateAsync(biller);

            return Ok(updatedBiller);
        }

        /// <summary>
        /// Active ou désactive un créancier
        /// </summary>
        /// <param name="code">Code unique du créancier</param>
        /// <param name="active">État d'activation</param>
        /// <returns>Résultat de l'opération</returns>
        [HttpPatch("{code}/activate")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ActivateBiller(string code, [FromQuery] bool active)
        {
            // Vérifier si le créancier existe
            var existing = await _billerRepository.GetByCodeAsync(code);
            if (existing == null)
            {
                return NotFound();
            }

            existing.IsActive = active;
            existing.UpdatedAt = DateTime.UtcNow;

            await _billerRepository.UpdateAsync(existing);

            return Ok(new { Status = "Success", Message = $"Créancier '{code}' {(active ? "activé" : "désactivé")}" });
        }
        /// <summary>
        /// Supprime un créancier
        /// </summary>
        /// <param name="code">Code unique du créancier</param>
        /// <returns>Résultat de l'opération</returns>
        [HttpDelete("{code}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteBiller(string code)
        {
            // Vérifier si le créancier existe
            var existing = await _billerRepository.GetByCodeAsync(code);
            if (existing == null)
            {
                return NotFound();
            }

            await _billerRepository.DeleteAsync(existing);

            return Ok(new { Status = "Success", Message = $"Créancier '{code}' supprimé" });
        }

        /// <summary>
        /// Récupère tous les créanciers d'une catégorie spécifique
        /// </summary>
        /// <param name="category">Catégorie de créancier</param>
        /// <returns>Liste des créanciers de la catégorie</returns>
        [HttpGet("category/{category}")]
        [ProducesResponseType(typeof(List<BillerConfiguration>), 200)]
        public async Task<IActionResult> GetBillersByCategory(string category)
        {
            var billers = await _billerRepository.GetByCategoryAsync(category);
            return Ok(billers);
        }

        /// <summary>
        /// Récupère tous les créanciers d'un type de service spécifique
        /// </summary>
        /// <param name="serviceType">Type de service (BILL_PAYMENT, TELECOM_RECHARGE)</param>
        /// <returns>Liste des créanciers du type de service</returns>
        [HttpGet("service-type/{serviceType}")]
        [ProducesResponseType(typeof(List<BillerConfiguration>), 200)]
        public async Task<IActionResult> GetBillersByServiceType(string serviceType)
        {
            var billers = await _billerRepository.GetByServiceTypeAsync(serviceType);
            return Ok(billers);
        }
    }
}
