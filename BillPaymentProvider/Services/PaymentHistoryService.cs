using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Data;
using Microsoft.EntityFrameworkCore;

namespace BillPaymentProvider.Services
{
    /// <summary>
    /// Service pour la gestion de l'historique des paiements
    /// </summary>
    public class PaymentHistoryService
    {
        private readonly AppDbContext _dbContext;
        private readonly IAppLogger _logger;

        public PaymentHistoryService(AppDbContext dbContext, IAppLogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Enregistre un historique de paiement
        /// </summary>
        public async Task<PaymentHistory> SaveAsync(B3gServiceRequest request, B3gServiceResponse response)
        {
            try
            {
                // Extraire les informations de la requête et de la réponse
                string? customerReference = null;
                string? phoneNumber = null;
                decimal amount = 0;
                string? transactionId = null;

                // Récupérer les paramètres de la requête
                if (request.ParamIn.TryGetValue("CustomerReference", out var customerRefObj))
                {
                    customerReference = customerRefObj.ToString();
                }

                if (request.ParamIn.TryGetValue("PhoneNumber", out var phoneObj))
                {
                    phoneNumber = phoneObj.ToString();
                }

                if (request.ParamIn.TryGetValue("Amount", out var amountObj))
                {
                    if (decimal.TryParse(amountObj.ToString(), out var parsedAmount))
                    {
                        amount = parsedAmount;
                    }
                }

                if (request.ParamIn.TryGetValue("BillerCode", out var billerCodeObj))
                {
                    // OK
                }
                else
                {
                    throw new InvalidOperationException("BillerCode manquant dans la requête");
                }

                // Récupérer les paramètres de la réponse
                if (response.ParamOut != null)
                {
                    var paramOut = response.ParamOut as dynamic;
                    if (paramOut != null && paramOut.TransactionId != null)
                    {
                        transactionId = paramOut.TransactionId.ToString();
                    }
                }

                // Créer l'entrée d'historique
                var history = new PaymentHistory
                {
                    SessionId = request.SessionId,
                    ServiceId = request.ServiceId,
                    BillerCode = billerCodeObj.ToString(),
                    CustomerReference = customerReference,
                    PhoneNumber = phoneNumber,
                    Amount = amount,
                    StatusCode = response.StatusCode,
                    ChannelId = request.ChannelId,
                    TransactionId = transactionId,
                    Timestamp = DateTime.UtcNow
                };

                // Enregistrer dans la base de données
                _dbContext.PaymentHistories.Add(history);
                await _dbContext.SaveChangesAsync();

                _logger.LogInfo($"Historique de paiement enregistré: SessionId={request.SessionId}, StatusCode={response.StatusCode}");

                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de l'enregistrement de l'historique: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère l'historique des paiements pour un client
        /// </summary>
        public async Task<List<PaymentHistory>> GetHistoryByCustomerReferenceAsync(
            string customerReference,
            int skip = 0,
            int take = 20)
        {
            try
            {
                return await _dbContext.PaymentHistories
                    .Where(h => h.CustomerReference == customerReference)
                    .OrderByDescending(h => h.Timestamp)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération de l'historique pour {customerReference}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère l'historique des paiements pour un numéro de téléphone
        /// </summary>
        public async Task<List<PaymentHistory>> GetHistoryByPhoneNumberAsync(
            string phoneNumber,
            int skip = 0,
            int take = 20)
        {
            try
            {
                return await _dbContext.PaymentHistories
                    .Where(h => h.PhoneNumber == phoneNumber)
                    .OrderByDescending(h => h.Timestamp)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération de l'historique pour {phoneNumber}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère l'historique des paiements pour un créancier
        /// </summary>
        public async Task<List<PaymentHistory>> GetHistoryByBillerCodeAsync(
            string billerCode,
            int skip = 0,
            int take = 20)
        {
            try
            {
                return await _dbContext.PaymentHistories
                    .Where(h => h.BillerCode == billerCode)
                    .OrderByDescending(h => h.Timestamp)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération de l'historique pour {billerCode}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère l'historique par ID de transaction
        /// </summary>
        public async Task<List<PaymentHistory>> GetHistoryByTransactionIdAsync(string transactionId)
        {
            try
            {
                return await _dbContext.PaymentHistories
                    .Where(h => h.TransactionId == transactionId)
                    .OrderByDescending(h => h.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération de l'historique pour la transaction {transactionId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère l'historique par ID de session
        /// </summary>
        public async Task<PaymentHistory?> GetHistoryBySessionIdAsync(string sessionId)
        {
            try
            {
                return await _dbContext.PaymentHistories
                    .FirstOrDefaultAsync(h => h.SessionId == sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération de l'historique pour la session {sessionId}: {ex.Message}");
                throw;
            }
        }
    }
}
