using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BillPaymentProvider.Data.Repositories
{
    /// <summary>
    /// Implémentation du dépôt de transactions
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _dbContext;

        public TransactionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Récupère une transaction par son ID
        /// </summary>
        public async Task<Transaction?> GetByTransactionIdAsync(string transactionId)
        {
            return await _dbContext.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        /// <summary>
        /// Récupère une transaction par son ID de session
        /// </summary>
        public async Task<Transaction?> GetBySessionIdAsync(string sessionId)
        {
            return await _dbContext.Transactions
                .FirstOrDefaultAsync(t => t.SessionId == sessionId);
        }

        /// <summary>
        /// Crée une nouvelle transaction
        /// </summary>
        public async Task<Transaction> CreateAsync(Transaction transaction)
        {
            _dbContext.Transactions.Add(transaction);
            await _dbContext.SaveChangesAsync();

            return transaction;
        }

        /// <summary>
        /// Met à jour une transaction existante
        /// </summary>
        public async Task<Transaction> UpdateAsync(Transaction transaction)
        {
            _dbContext.Transactions.Update(transaction);
            await _dbContext.SaveChangesAsync();

            return transaction;
        }

        /// <summary>
        /// Récupère l'historique des transactions d'un client
        /// </summary>
        public async Task<List<Transaction>> GetTransactionHistoryAsync(string customerReference, int skip = 0, int take = 20)
        {
            return await _dbContext.Transactions
                .Where(t => t.CustomerReference == customerReference)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère l'historique des transactions d'un numéro de téléphone
        /// </summary>
        public async Task<List<Transaction>> GetPhoneTransactionHistoryAsync(string phoneNumber, int skip = 0, int take = 20)
        {
            return await _dbContext.Transactions
                .Where(t => t.PhoneNumber == phoneNumber)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les transactions par statut
        /// </summary>
        public async Task<List<Transaction>> GetByStatusAsync(string status, int skip = 0, int take = 100)
        {
            return await _dbContext.Transactions
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <summary>
        /// Enregistre un événement dans le log de transaction
        /// </summary>
        public async Task LogTransactionEventAsync(TransactionLog logEntry)
        {
            _dbContext.TransactionLogs.Add(logEntry);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Récupère les logs d'une transaction
        /// </summary>
        public async Task<List<TransactionLog>> GetTransactionLogsAsync(string transactionId)
        {
            return await _dbContext.TransactionLogs
                .Where(l => l.TransactionId == transactionId)
                .OrderBy(l => l.Timestamp)
                .ToListAsync();
        }
    }
}
