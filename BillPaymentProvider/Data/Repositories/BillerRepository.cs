using BillPaymentProvider.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BillPaymentProvider.Data.Repositories
{
    /// <summary>
    /// Dépôt pour la gestion des créanciers en base de données
    /// </summary>
    public class BillerRepository
    {
        private readonly AppDbContext _dbContext;

        public BillerRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Récupère tous les créanciers
        /// </summary>
        public async Task<List<BillerConfiguration>> GetAllAsync()
        {
            return await _dbContext.BillerConfigurations
                .OrderBy(b => b.BillerName)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère un créancier par son code
        /// </summary>
        public async Task<BillerConfiguration?> GetByCodeAsync(string billerCode)
        {
            return await _dbContext.BillerConfigurations
                .FirstOrDefaultAsync(b => b.BillerCode == billerCode);
        }

        /// <summary>
        /// Récupère les créanciers par catégorie
        /// </summary>
        public async Task<List<BillerConfiguration>> GetByCategoryAsync(string category)
        {
            return await _dbContext.BillerConfigurations
                .Where(b => b.Category == category)
                .OrderBy(b => b.BillerName)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les créanciers par type de service
        /// </summary>
        public async Task<List<BillerConfiguration>> GetByServiceTypeAsync(string serviceType)
        {
            return await _dbContext.BillerConfigurations
                .Where(b => b.ServiceType == serviceType)
                .OrderBy(b => b.BillerName)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les créanciers actifs
        /// </summary>
        public async Task<List<BillerConfiguration>> GetActiveAsync()
        {
            return await _dbContext.BillerConfigurations
                .Where(b => b.IsActive)
                .OrderBy(b => b.BillerName)
                .ToListAsync();
        }

        /// <summary>
        /// Crée un nouveau créancier
        /// </summary>
        public async Task<BillerConfiguration> CreateAsync(BillerConfiguration biller)
        {
            biller.CreatedAt = DateTime.UtcNow;
            biller.UpdatedAt = null;

            _dbContext.BillerConfigurations.Add(biller);
            await _dbContext.SaveChangesAsync();

            return biller;
        }

        /// <summary>
        /// Met à jour un créancier existant
        /// </summary>
        public async Task<BillerConfiguration> UpdateAsync(BillerConfiguration biller)
        {
            biller.UpdatedAt = DateTime.UtcNow;

            _dbContext.BillerConfigurations.Update(biller);
            await _dbContext.SaveChangesAsync();

            return biller;
        }

        /// <summary>
        /// Supprime un créancier
        /// </summary>
        public async Task DeleteAsync(BillerConfiguration biller)
        {
            _dbContext.BillerConfigurations.Remove(biller);
            await _dbContext.SaveChangesAsync();
        }
    }
}
