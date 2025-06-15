using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Data.Repositories;

namespace BillPaymentProvider.Services
{
    /// <summary>
    /// Service pour la gestion des configurations de créanciers
    /// </summary>
    public class BillerConfigService
    {
        private readonly BillerRepository _billerRepository;
        private readonly IAppLogger _logger;

        public BillerConfigService(BillerRepository billerRepository, IAppLogger logger)
        {
            _billerRepository = billerRepository;
            _logger = logger;
        }

        /// <summary>
        /// Récupère tous les créanciers
        /// </summary>
        public async Task<List<BillerConfiguration>> GetAllBillersAsync()
        {
            try
            {
                return await _billerRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération des créanciers: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère un créancier par son code
        /// </summary>
        public async Task<BillerConfiguration?> GetBillerByCodeAsync(string billerCode)
        {
            try
            {
                return await _billerRepository.GetByCodeAsync(billerCode);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération du créancier {billerCode}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère les créanciers par catégorie
        /// </summary>
        public async Task<List<BillerConfiguration>> GetBillersByCategoryAsync(string category)
        {
            try
            {
                return await _billerRepository.GetByCategoryAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération des créanciers de catégorie {category}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère les créanciers par type de service
        /// </summary>
        public async Task<List<BillerConfiguration>> GetBillersByServiceTypeAsync(string serviceType)
        {
            try
            {
                return await _billerRepository.GetByServiceTypeAsync(serviceType);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération des créanciers de type {serviceType}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Crée un nouveau créancier
        /// </summary>
        public async Task<BillerConfiguration> CreateBillerAsync(BillerConfiguration biller)
        {
            try
            {
                // Vérifier si le code est unique
                var existing = await _billerRepository.GetByCodeAsync(biller.BillerCode);
                if (existing != null)
                {
                    throw new InvalidOperationException($"Un créancier avec le code '{biller.BillerCode}' existe déjà");
                }

                // Définir les dates
                biller.CreatedAt = DateTime.UtcNow;
                biller.UpdatedAt = null;

                var createdBiller = await _billerRepository.CreateAsync(biller);
                _logger.LogInfo($"Créancier créé: {biller.BillerCode} - {biller.BillerName}");

                return createdBiller;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la création du créancier {biller.BillerCode}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Met à jour un créancier existant
        /// </summary>
        public async Task<BillerConfiguration> UpdateBillerAsync(string billerCode, BillerConfiguration biller)
        {
            try
            {
                // Vérifier si le créancier existe
                var existing = await _billerRepository.GetByCodeAsync(billerCode);
                if (existing == null)
                {
                    throw new InvalidOperationException($"Créancier avec le code '{billerCode}' non trouvé");
                }

                // Mettre à jour l'ID et le code
                biller.Id = existing.Id;
                biller.BillerCode = billerCode; // S'assurer que le code ne change pas
                biller.CreatedAt = existing.CreatedAt; // Conserver la date de création
                biller.UpdatedAt = DateTime.UtcNow;

                var updatedBiller = await _billerRepository.UpdateAsync(biller);
                _logger.LogInfo($"Créancier mis à jour: {biller.BillerCode} - {biller.BillerName}");

                return updatedBiller;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la mise à jour du créancier {billerCode}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Supprime un créancier
        /// </summary>
        public async Task DeleteBillerAsync(string billerCode)
        {
            try
            {
                // Vérifier si le créancier existe
                var existing = await _billerRepository.GetByCodeAsync(billerCode);
                if (existing == null)
                {
                    throw new InvalidOperationException($"Créancier avec le code '{billerCode}' non trouvé");
                }

                await _billerRepository.DeleteAsync(existing);
                _logger.LogInfo($"Créancier supprimé: {billerCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la suppression du créancier {billerCode}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Active ou désactive un créancier
        /// </summary>
        public async Task<BillerConfiguration> SetBillerActiveStatusAsync(string billerCode, bool active)
        {
            try
            {
                // Vérifier si le créancier existe
                var existing = await _billerRepository.GetByCodeAsync(billerCode);
                if (existing == null)
                {
                    throw new InvalidOperationException($"Créancier avec le code '{billerCode}' non trouvé");
                }

                // Mettre à jour le statut
                existing.IsActive = active;
                existing.UpdatedAt = DateTime.UtcNow;

                var updatedBiller = await _billerRepository.UpdateAsync(existing);
                _logger.LogInfo($"Statut du créancier {billerCode} modifié: IsActive={active}");

                return updatedBiller;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la modification du statut du créancier {billerCode}: {ex.Message}");
                throw;
            }
        }
    }
}
