using BillPaymentProvider.Core.Enums;
using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Core.Models;

namespace BillPaymentProvider.Services
{
    /// <summary>
    /// Service pour la gestion des transactions
    /// </summary>
    public class TransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAppLogger _logger;

        public TransactionService(ITransactionRepository transactionRepository, IAppLogger logger)
        {
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        /// <summary>
        /// Récupère une transaction par son ID
        /// </summary>
        public async Task<Transaction?> GetByTransactionIdAsync(string transactionId)
        {
            try
            {
                return await _transactionRepository.GetByTransactionIdAsync(transactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération de la transaction {transactionId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère une transaction par son ID de session
        /// </summary>
        public async Task<Transaction?> GetBySessionIdAsync(string sessionId)
        {
            try
            {
                return await _transactionRepository.GetBySessionIdAsync(sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération de la transaction pour la session {sessionId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Crée une nouvelle transaction
        /// </summary>
        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            try
            {
                // S'assurer que l'ID de transaction est unique
                if (string.IsNullOrEmpty(transaction.TransactionId))
                {
                    transaction.TransactionId = Guid.NewGuid().ToString("N");
                }

                // Définir les dates
                transaction.CreatedAt = DateTime.UtcNow;

                // Si le statut est COMPLETED, définir la date de complétion
                if (transaction.Status == TransactionStatus.COMPLETED)
                {
                    transaction.CompletedAt = DateTime.UtcNow;
                }

                var createdTransaction = await _transactionRepository.CreateAsync(transaction);

                // Créer un log pour cette transaction
                var logEntry = new TransactionLog
                {
                    TransactionId = transaction.TransactionId,
                    Action = transaction.PhoneNumber != null ? "RECHARGE" : "PAYMENT",
                    Details = transaction.PhoneNumber != null
                        ? $"Recharge {transaction.BillerCode} pour {transaction.PhoneNumber} montant {transaction.Amount}"
                        : $"Paiement {transaction.BillerCode} pour {transaction.CustomerReference} montant {transaction.Amount}",
                    NewStatus = transaction.Status,
                    Timestamp = DateTime.UtcNow
                };

                await _transactionRepository.LogTransactionEventAsync(logEntry);

                _logger.LogInfo($"Transaction créée: {transaction.TransactionId}, Status={transaction.Status}");

                return createdTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la création de la transaction: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Met à jour une transaction existante
        /// </summary>
        public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
        {
            try
            {
                var existingTransaction = await _transactionRepository.GetByTransactionIdAsync(transaction.TransactionId);
                if (existingTransaction == null)
                {
                    throw new InvalidOperationException($"Transaction {transaction.TransactionId} non trouvée");
                }

                // Garder l'historique des changements de statut
                if (existingTransaction.Status != transaction.Status)
                {
                    var logEntry = new TransactionLog
                    {
                        TransactionId = transaction.TransactionId,
                        Action = "STATUS_CHANGE",
                        Details = $"Changement de statut: {existingTransaction.Status} -> {transaction.Status}",
                        PreviousStatus = existingTransaction.Status,
                        NewStatus = transaction.Status,
                        Timestamp = DateTime.UtcNow
                    };

                    await _transactionRepository.LogTransactionEventAsync(logEntry);

                    // Mettre à jour les dates selon le nouveau statut
                    if (transaction.Status == TransactionStatus.COMPLETED && existingTransaction.CompletedAt == null)
                    {
                        transaction.CompletedAt = DateTime.UtcNow;
                    }
                    else if (transaction.Status == TransactionStatus.CANCELLED && existingTransaction.CancelledAt == null)
                    {
                        transaction.CancelledAt = DateTime.UtcNow;
                    }
                }

                // Mettre à jour la transaction
                var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);

                _logger.LogInfo($"Transaction mise à jour: {transaction.TransactionId}, Status={transaction.Status}");

                return updatedTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la mise à jour de la transaction: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Change le statut d'une transaction
        /// </summary>
        public async Task<Transaction> ChangeTransactionStatusAsync(string transactionId, string newStatus, string? reason = null)
        {
            try
            {
                var transaction = await _transactionRepository.GetByTransactionIdAsync(transactionId);
                if (transaction == null)
                {
                    throw new InvalidOperationException($"Transaction {transactionId} non trouvée");
                }

                // Vérifier si le changement est autorisé
                ValidateStatusChange(transaction.Status, newStatus);

                // Sauvegarder l'ancien statut
                var oldStatus = transaction.Status;

                // Mettre à jour le statut
                transaction.Status = newStatus;

                // Mettre à jour les dates et la raison selon le nouveau statut
                if (newStatus == TransactionStatus.COMPLETED && transaction.CompletedAt == null)
                {
                    transaction.CompletedAt = DateTime.UtcNow;
                }
                else if (newStatus == TransactionStatus.CANCELLED && transaction.CancelledAt == null)
                {
                    transaction.CancelledAt = DateTime.UtcNow;

                    if (!string.IsNullOrEmpty(reason))
                    {
                        transaction.FailureReason = reason;
                    }
                }
                else if (newStatus == TransactionStatus.FAILED && string.IsNullOrEmpty(transaction.FailureReason))
                {
                    transaction.FailureReason = reason ?? "Échec de la transaction";
                }

                // Mettre à jour la transaction
                var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);

                // Créer un log pour ce changement de statut
                var logEntry = new TransactionLog
                {
                    TransactionId = transactionId,
                    Action = "STATUS_CHANGE",
                    Details = !string.IsNullOrEmpty(reason)
                        ? $"Changement de statut: {oldStatus} -> {newStatus}. Raison: {reason}"
                        : $"Changement de statut: {oldStatus} -> {newStatus}",
                    PreviousStatus = oldStatus,
                    NewStatus = newStatus,
                    Timestamp = DateTime.UtcNow
                };

                await _transactionRepository.LogTransactionEventAsync(logEntry);

                _logger.LogInfo($"Statut de la transaction {transactionId} modifié: {oldStatus} -> {newStatus}");

                return updatedTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors du changement de statut de la transaction {transactionId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Annule une transaction
        /// </summary>
        public async Task<Transaction> CancelTransactionAsync(string transactionId, string reason)
        {
            return await ChangeTransactionStatusAsync(transactionId, TransactionStatus.CANCELLED, reason);
        }

        /// <summary>
        /// Récupère l'historique des transactions d'un client
        /// </summary>
        public async Task<List<Transaction>> GetTransactionHistoryAsync(string customerReference, int skip = 0, int take = 20)
        {
            try
            {
                return await _transactionRepository.GetTransactionHistoryAsync(customerReference, skip, take);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération de l'historique pour {customerReference}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère l'historique des transactions pour un numéro de téléphone
        /// </summary>
        public async Task<List<Transaction>> GetPhoneTransactionHistoryAsync(string phoneNumber, int skip = 0, int take = 20)
        {
            try
            {
                return await _transactionRepository.GetPhoneTransactionHistoryAsync(phoneNumber, skip, take);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération de l'historique pour {phoneNumber}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère les logs d'une transaction
        /// </summary>
        public async Task<List<TransactionLog>> GetTransactionLogsAsync(string transactionId)
        {
            try
            {
                return await _transactionRepository.GetTransactionLogsAsync(transactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération des logs pour la transaction {transactionId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Vérifie si un changement de statut est autorisé
        /// </summary>
        private void ValidateStatusChange(string currentStatus, string newStatus)
        {
            // Règles de transition de statut
            if (currentStatus == TransactionStatus.CANCELLED || currentStatus == TransactionStatus.REFUNDED)
            {
                throw new InvalidOperationException($"Impossible de changer le statut d'une transaction {currentStatus}");
            }

            if (currentStatus == TransactionStatus.COMPLETED)
            {
                if (newStatus != TransactionStatus.CANCELLED && newStatus != TransactionStatus.REFUNDING)
                {
                    throw new InvalidOperationException($"Une transaction {currentStatus} ne peut passer qu'à {TransactionStatus.CANCELLED} ou {TransactionStatus.REFUNDING}");
                }
            }

            if (currentStatus == TransactionStatus.FAILED)
            {
                if (newStatus != TransactionStatus.PENDING)
                {
                    throw new InvalidOperationException($"Une transaction {currentStatus} ne peut passer qu'à {TransactionStatus.PENDING}");
                }
            }

            if (currentStatus == TransactionStatus.REFUNDING)
            {
                if (newStatus != TransactionStatus.REFUNDED && newStatus != TransactionStatus.FAILED)
                {
                    throw new InvalidOperationException($"Une transaction {currentStatus} ne peut passer qu'à {TransactionStatus.REFUNDED} ou {TransactionStatus.FAILED}");
                }
            }
        }
    }
}
