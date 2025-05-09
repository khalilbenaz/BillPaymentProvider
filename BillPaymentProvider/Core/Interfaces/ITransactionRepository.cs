using BillPaymentProvider.Core.Models;

namespace BillPaymentProvider.Core.Interfaces
{
    /// <summary>
    /// Interface pour le dépôt de transactions
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Récupère une transaction par son ID unique
        /// </summary>
        Task<Transaction?> GetByTransactionIdAsync(string transactionId);

        /// <summary>
        /// Récupère une transaction par son ID de session
        /// </summary>
        Task<Transaction?> GetBySessionIdAsync(string sessionId);

        /// <summary>
        /// Crée une nouvelle transaction
        /// </summary>
        Task<Transaction> CreateAsync(Transaction transaction);

        /// <summary>
        /// Met à jour une transaction existante
        /// </summary>
        Task<Transaction> UpdateAsync(Transaction transaction);

        /// <summary>
        /// Récupère l'historique des transactions d'un client
        /// </summary>
        Task<List<Transaction>> GetTransactionHistoryAsync(string customerReference, int skip = 0, int take = 20);

        /// <summary>
        /// Récupère l'historique des transactions d'un numéro de téléphone
        /// </summary>
        Task<List<Transaction>> GetPhoneTransactionHistoryAsync(string phoneNumber, int skip = 0, int take = 20);

        /// <summary>
        /// Récupère les transactions par statut
        /// </summary>
        Task<List<Transaction>> GetByStatusAsync(string status, int skip = 0, int take = 100);

        /// <summary>
        /// Enregistre un événement dans le log de transaction
        /// </summary>
        Task LogTransactionEventAsync(TransactionLog logEntry);

        /// <summary>
        /// Récupère les logs d'une transaction
        /// </summary>
        Task<List<TransactionLog>> GetTransactionLogsAsync(string transactionId);
    }
}
