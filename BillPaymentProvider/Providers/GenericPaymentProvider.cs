using BillPaymentProvider.Core.Enums;
using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Data.Repositories;
using BillPaymentProvider.Utils;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BillPaymentProvider.Providers
{
    /// <summary>
    /// Provider générique qui traite tous les types de paiement
    /// Le comportement est déterminé par la configuration du créancier en base de données
    /// </summary>
    public class GenericPaymentProvider
    {
        private readonly BillerRepository _billerRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAppLogger _logger;
        private readonly Random _random = new();

        public GenericPaymentProvider(
            BillerRepository billerRepository,
            ITransactionRepository transactionRepository,
            IAppLogger logger)
        {
            _billerRepository = billerRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        /// <summary>
        /// Vérifie si une facture ou un service est disponible pour la référence client donnée
        /// </summary>
        public async Task<B3gServiceResponse> InquireAsync(B3gServiceRequest request)
        {
            // Validation de base
            if (!request.ParamIn.TryGetValue("BillerCode", out var billerCodeObj))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.MISSING_PARAMETER, "BillerCode manquant");
            }

            var billerCode = billerCodeObj?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(billerCode))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.MISSING_PARAMETER, "BillerCode manquant");
            }

            // Récupérer la configuration du créancier
            var billerConfig = await _billerRepository.GetByCodeAsync(billerCode);
            if (billerConfig == null)
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.BILLER_NOT_FOUND, $"Créancier non trouvé: {billerCode}");
            }

            try
            {
                // Simuler un délai de traitement si configuré
                if (billerConfig.ProcessingDelay > 0)
                {
                    await Task.Delay(billerConfig.ProcessingDelay);
                }

                // Simuler des erreurs aléatoires si configuré
                if (billerConfig.SimulateRandomErrors && _random.Next(1, 100) <= billerConfig.ErrorRate)
                {
                    return SimulateRandomError(request);
                }

                // Logique différente selon le type de service
                if (billerConfig.ServiceType == ServiceType.BILL_PAYMENT)
                {
                    return await InquireBill(request, billerConfig);
                }
                else if (billerConfig.ServiceType == ServiceType.TELECOM_RECHARGE)
                {
                    return await InquireTelecom(request, billerConfig);
                }
                else
                {
                    return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.SERVICE_UNAVAILABLE, "Type de service non supporté");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de l'interrogation: {ex.Message}");
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.SYSTEM_ERROR, $"Erreur interne: {ex.Message}");
            }
        }

        /// <summary>
        /// Traite un paiement de facture ou une recharge télécom
        /// </summary>
        public async Task<B3gServiceResponse> ProcessPaymentAsync(B3gServiceRequest request)
        {
            // Validation de base
            if (!request.ParamIn.TryGetValue("BillerCode", out var billerCodeObj))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.MISSING_PARAMETER, "BillerCode manquant");
            }

            var billerCode = billerCodeObj?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(billerCode))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.MISSING_PARAMETER, "BillerCode manquant");
            }

            // Récupérer la configuration du créancier
            var billerConfig = await _billerRepository.GetByCodeAsync(billerCode);
            if (billerConfig == null)
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.BILLER_NOT_FOUND, $"Créancier non trouvé: {billerCode}");
            }

            try
            {
                // Vérifier si la transaction existe déjà (idempotence)
                var existingTransaction = await _transactionRepository.GetBySessionIdAsync(request.SessionId);
                if (existingTransaction != null)
                {
                    _logger.LogWarning($"Transaction dupliquée détectée: {request.SessionId}");

                    // Recréer la réponse à partir de la transaction existante
                    return await RecreateResponseFromTransaction(existingTransaction, request);
                }

                // Simuler un délai de traitement si configuré
                if (billerConfig.ProcessingDelay > 0)
                {
                    await Task.Delay(billerConfig.ProcessingDelay);
                }

                // Simuler des erreurs aléatoires si configuré
                if (billerConfig.SimulateRandomErrors && _random.Next(1, 100) <= billerConfig.ErrorRate)
                {
                    return SimulateRandomError(request);
                }

                // Logique différente selon le type de service
                if (billerConfig.ServiceType == ServiceType.BILL_PAYMENT)
                {
                    return await ProcessBillPayment(request, billerConfig);
                }
                else if (billerConfig.ServiceType == ServiceType.TELECOM_RECHARGE)
                {
                    return await ProcessTelecomRecharge(request, billerConfig);
                }
                else
                {
                    return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.SERVICE_UNAVAILABLE, "Type de service non supporté");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors du paiement: {ex.Message}");
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.SYSTEM_ERROR, $"Erreur interne: {ex.Message}");
            }
        }

        /// <summary>
        /// Vérifie le statut d'une transaction
        /// </summary>
        public async Task<B3gServiceResponse> CheckStatusAsync(B3gServiceRequest request)
        {
            // Validation de base
            if (!request.ParamIn.TryGetValue("TransactionId", out var transactionIdObj))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.MISSING_PARAMETER, "TransactionId manquant");
            }

            var transactionId = transactionIdObj?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(transactionId))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.MISSING_PARAMETER, "TransactionId manquant");
            }

            try
            {
                // Récupérer la transaction
                var transaction = await _transactionRepository.GetByTransactionIdAsync(transactionId);
                if (transaction == null)
                {
                    return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.TRANSACTION_NOT_FOUND, "Transaction non trouvée");
                }

                // Créer la réponse avec le statut
                var response = new B3gServiceResponse
                {
                    SessionId = request.SessionId,
                    ServiceId = request.ServiceId,
                    StatusCode = BillPaymentProvider.Core.Constants.StatusCodes.SUCCESS,
                    StatusLabel = "Statut récupéré avec succès",
                    ParamOut = new
                    {
                        TransactionId = transaction.TransactionId,
                        Status = transaction.Status,
                        BillerCode = transaction.BillerCode,
                        Amount = transaction.Amount,
                        CreatedAt = transaction.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        CompletedAt = transaction.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        ReceiptNumber = transaction.ReceiptNumber
                    }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la vérification de statut: {ex.Message}");
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.SYSTEM_ERROR, $"Erreur interne: {ex.Message}");
            }
        }

        /// <summary>
        /// Annule une transaction si c'est encore possible
        /// </summary>
        public async Task<B3gServiceResponse> CancelTransactionAsync(B3gServiceRequest request)
        {
            // Validation de base
            if (!request.ParamIn.TryGetValue("TransactionId", out var transactionIdObj))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.MISSING_PARAMETER, "TransactionId manquant");
            }

            var transactionId = transactionIdObj.ToString();

            try
            {
                // Récupérer la transaction
                var transaction = await _transactionRepository.GetByTransactionIdAsync(transactionId);
                if (transaction == null)
                {
                    return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.TRANSACTION_NOT_FOUND, "Transaction non trouvée");
                }

                // Vérifier si la transaction peut être annulée
                if (transaction.Status != TransactionStatus.COMPLETED &&
                    transaction.Status != TransactionStatus.PENDING)
                {
                    return CreateErrorResponse(
                        request,
                        BillPaymentProvider.Core.Constants.StatusCodes.CANNOT_CANCEL,
                        $"Impossible d'annuler une transaction en statut {transaction.Status}"
                    );
                }

                // Vérifier si la transaction est trop ancienne pour être annulée (24h max)
                var timeSinceCreation = DateTime.UtcNow - transaction.CreatedAt;
                if (timeSinceCreation.TotalHours > 24)
                {
                    return CreateErrorResponse(
                        request,
                        BillPaymentProvider.Core.Constants.StatusCodes.CANNOT_CANCEL,
                        "Impossible d'annuler une transaction de plus de 24 heures"
                    );
                }

                // Mettre à jour la transaction
                transaction.Status = TransactionStatus.CANCELLED;
                transaction.CancelledAt = DateTime.UtcNow;
                await _transactionRepository.UpdateAsync(transaction);

                // Enregistrer l'événement dans le log
                var logEntry = new TransactionLog
                {
                    TransactionId = transactionId,
                    Action = "CANCEL",
                    Details = $"Annulation de la transaction",
                    PreviousStatus = transaction.Status,
                    NewStatus = TransactionStatus.CANCELLED,
                    Timestamp = DateTime.UtcNow
                };

                await _transactionRepository.LogTransactionEventAsync(logEntry);

                // Créer la réponse
                var response = new B3gServiceResponse
                {
                    SessionId = request.SessionId,
                    ServiceId = request.ServiceId,
                    StatusCode = BillPaymentProvider.Core.Constants.StatusCodes.SUCCESS,
                    StatusLabel = "Transaction annulée avec succès",
                    ParamOut = new
                    {
                        TransactionId = transaction.TransactionId,
                        Status = TransactionStatus.CANCELLED,
                        BillerCode = transaction.BillerCode,
                        Amount = transaction.Amount,
                        CancelledAt = transaction.CancelledAt?.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de l'annulation: {ex.Message}");
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.SYSTEM_ERROR, $"Erreur interne: {ex.Message}");
            }
        }

        #region Private Methods

        /// <summary>
        /// Détecte la langue de la requête
        /// </summary>
        private string GetLang(B3gServiceRequest request)
        {
            if (!string.IsNullOrEmpty(request.Language))
                return request.Language.ToLower();
            if (request.ParamIn != null && request.ParamIn.TryGetValue("Language", out var langObj) && langObj != null && !string.IsNullOrEmpty(langObj.ToString()))
                return langObj.ToString().ToLower();
            return "fr";
        }

        /// <summary>
        /// Traite une demande d'information pour une facture
        /// </summary>
        private async Task<B3gServiceResponse> InquireBill(B3gServiceRequest request, BillerConfiguration billerConfig)
        {
            // Validation des paramètres
            if (!request.ParamIn.TryGetValue("CustomerReference", out var customerRefObj))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.MISSING_PARAMETER, "CustomerReference manquant");
            }

            var customerRef = customerRefObj.ToString();

            // Valider le format de référence client si configuré
            if (!string.IsNullOrEmpty(billerConfig.CustomerReferenceFormat) &&
                !Regex.IsMatch(customerRef, billerConfig.CustomerReferenceFormat))
            {
                return CreateErrorResponse(
                    request,
                    BillPaymentProvider.Core.Constants.StatusCodes.INVALID_REFERENCE,
                    "Format de référence client invalide"
                );
            }

            // Simuler une facture
            
            var dueAmount = GenerateRandomAmount(50, 500);
            var dueDate = DateTime.UtcNow.AddDays(_random.Next(5, 30));
            var customerName = GenerateRandomCustomerName();

            var response = new B3gServiceResponse
            {
                SessionId = request.SessionId,
                ServiceId = request.ServiceId,
                StatusCode = BillPaymentProvider.Core.Constants.StatusCodes.SUCCESS,
                StatusLabel = "Facture trouvée",
                ParamOut = new
                {
                    BillerCode = billerConfig.BillerCode,
                    BillerName = billerConfig.BillerName,
                    CustomerReference = customerRef,
                    CustomerName = customerName,
                    DueAmount = dueAmount,
                    DueDate = dueDate.ToString("yyyy-MM-dd"),
                    BillPeriod = $"{DateTime.UtcNow.AddMonths(-1):MMM yyyy} - {DateTime.UtcNow:MMM yyyy}",
                    BillNumber = GenerateRandomBillNumber()
                }
            };

            return response;
        }

        /// <summary>
        /// Traite une demande d'information pour une recharge télécom
        /// </summary>
        private async Task<B3gServiceResponse> InquireTelecom(B3gServiceRequest request, BillerConfiguration billerConfig)
        {
            // Validation des paramètres
            if (!request.ParamIn.TryGetValue("PhoneNumber", out var phoneObj))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.MISSING_PARAMETER, "PhoneNumber manquant");
            }

            var phoneNumber = phoneObj.ToString();

            // Valider le format de téléphone si configuré
            if (!string.IsNullOrEmpty(billerConfig.PhoneNumberFormat) &&
                !Regex.IsMatch(phoneNumber, billerConfig.PhoneNumberFormat))
            {
                return CreateErrorResponse(
                    request,
                    BillPaymentProvider.Core.Constants.StatusCodes.INVALID_PHONE,
                    "Format de numéro de téléphone invalide"
                );
            }

            // Récupérer les montants disponibles
            var availableAmounts = billerConfig.GetAvailableAmounts();

            var response = new B3gServiceResponse
            {
                SessionId = request.SessionId,
                ServiceId = request.ServiceId,
                StatusCode = BillPaymentProvider.Core.Constants.StatusCodes.SUCCESS,
                StatusLabel = "Numéro validé",
                ParamOut = new
                {
                    BillerCode = billerConfig.BillerCode,
                    BillerName = billerConfig.BillerName,
                    PhoneNumber = phoneNumber,
                    AvailableAmounts = availableAmounts,
                    MinAmount = availableAmounts.Any() ? availableAmounts.Min() : 0,
                    MaxAmount = availableAmounts.Any() ? availableAmounts.Max() : 0
                }
            };

            return response;
        }

        /// <summary>
        /// Traite un paiement de facture
        /// </summary>
        private async Task<B3gServiceResponse> ProcessBillPayment(B3gServiceRequest request, BillerConfiguration billerConfig)
        {
            // Validation des paramètres
            if (!request.ParamIn.TryGetValue("CustomerReference", out var customerRefObj))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.MISSING_PARAMETER, "CustomerReference manquant");
            }

            if (!request.ParamIn.TryGetValue("Amount", out var amountObj) ||
                !decimal.TryParse(amountObj.ToString(), out var amount))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.INVALID_AMOUNT, "Montant invalide ou manquant");
            }

            var customerRef = customerRefObj.ToString();

            // Valider le format de référence client si configuré
            if (!string.IsNullOrEmpty(billerConfig.CustomerReferenceFormat) &&
                !Regex.IsMatch(customerRef, billerConfig.CustomerReferenceFormat))
            {
                return CreateErrorResponse(
                    request,
                    BillPaymentProvider.Core.Constants.StatusCodes.INVALID_REFERENCE,
                    "Format de référence client invalide"
                );
            }

            // Créer une nouvelle transaction
            var transactionId = Guid.NewGuid().ToString("N");
            var receiptNumber = GenerateReceiptNumber();

            var transaction = new Transaction
            {
                TransactionId = transactionId,
                BillerCode = billerConfig.BillerCode,
                CustomerReference = customerRef,
                Amount = amount,
                Status = TransactionStatus.COMPLETED,
                SessionId = request.SessionId,
                ServiceId = request.ServiceId,
                ChannelId = request.ChannelId ?? PaymentChannel.API,
                ReceiptNumber = receiptNumber,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                RawRequest = JsonSerializer.Serialize(request)
            };

            // Enregistrer la transaction
            await _transactionRepository.CreateAsync(transaction);

            // Créer la log d'événement
            var logEntry = new TransactionLog
            {
                TransactionId = transactionId,
                Action = "PAYMENT",
                Details = $"Paiement {billerConfig.BillerCode} pour {customerRef} montant {amount}",
                NewStatus = TransactionStatus.COMPLETED,
                Timestamp = DateTime.UtcNow
            };

            await _transactionRepository.LogTransactionEventAsync(logEntry);

            var response = new B3gServiceResponse
            {
                SessionId = request.SessionId,
                ServiceId = request.ServiceId,
                StatusCode = BillPaymentProvider.Core.Constants.StatusCodes.SUCCESS,
                StatusLabel = $"Paiement {billerConfig.BillerName} effectué avec succès",
                ParamOut = new
                {
                    TransactionId = transactionId,
                    ReceiptNumber = receiptNumber,
                    CustomerReference = customerRef,
                    BillerCode = billerConfig.BillerCode,
                    BillerName = billerConfig.BillerName,
                    Amount = amount,
                    PaymentDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = TransactionStatus.COMPLETED
                }
            };

            // Mettre à jour la transaction avec la réponse
            transaction.RawResponse = JsonSerializer.Serialize(response);
            await _transactionRepository.UpdateAsync(transaction);

            _logger.LogInfo($"Paiement simulé avec succès pour {billerConfig.BillerCode}, Référence: {customerRef}, Montant: {amount}");
            return response;
        }

        /// <summary>
        /// Traite une recharge télécom
        /// </summary>
        private async Task<B3gServiceResponse> ProcessTelecomRecharge(B3gServiceRequest request, BillerConfiguration billerConfig)
        {
            // Validation des paramètres
            if (!request.ParamIn.TryGetValue("PhoneNumber", out var phoneObj))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.MISSING_PARAMETER, "PhoneNumber manquant");
            }

            if (!request.ParamIn.TryGetValue("Amount", out var amountObj) ||
                !decimal.TryParse(amountObj.ToString(), out var amount))
            {
                return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.INVALID_AMOUNT, "Montant invalide ou manquant");
            }

            var phoneNumber = phoneObj.ToString();

            // Valider le format de téléphone si configuré
            if (!string.IsNullOrEmpty(billerConfig.PhoneNumberFormat) &&
                !Regex.IsMatch(phoneNumber, billerConfig.PhoneNumberFormat))
            {
                return CreateErrorResponse(
                    request,
                    BillPaymentProvider.Core.Constants.StatusCodes.INVALID_PHONE,
                    "Format de numéro de téléphone invalide"
                );
            }

            // Vérifier si le montant est dans la liste des montants disponibles
            var availableAmounts = billerConfig.GetAvailableAmounts();
            if (availableAmounts.Any() && !availableAmounts.Contains(amount))
            {
                return CreateErrorResponse(
                    request,
                    BillPaymentProvider.Core.Constants.StatusCodes.INVALID_AMOUNT,
                    $"Montant invalide. Montants disponibles: {string.Join(", ", availableAmounts)}"
                );
            }

            // Créer une nouvelle transaction
            var transactionId = Guid.NewGuid().ToString("N");
            var receiptNumber = GenerateReceiptNumber();

            var transaction = new Transaction
            {
                TransactionId = transactionId,
                BillerCode = billerConfig.BillerCode,
                PhoneNumber = phoneNumber,
                Amount = amount,
                Status = TransactionStatus.COMPLETED,
                SessionId = request.SessionId,
                ServiceId = request.ServiceId,
                ChannelId = request.ChannelId ?? PaymentChannel.API,
                ReceiptNumber = receiptNumber,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                RawRequest = JsonSerializer.Serialize(request)
            };

            // Enregistrer la transaction
            await _transactionRepository.CreateAsync(transaction);

            // Créer la log d'événement
            var logEntry = new TransactionLog
            {
                TransactionId = transactionId,
                Action = "RECHARGE",
                Details = $"Recharge {billerConfig.BillerCode} pour {phoneNumber} montant {amount}",
                NewStatus = TransactionStatus.COMPLETED,
                Timestamp = DateTime.UtcNow
            };

            await _transactionRepository.LogTransactionEventAsync(logEntry);

            var response = new B3gServiceResponse
            {
                SessionId = request.SessionId,
                ServiceId = request.ServiceId,
                StatusCode = BillPaymentProvider.Core.Constants.StatusCodes.SUCCESS,
                StatusLabel = $"Recharge {billerConfig.BillerName} effectuée avec succès",
                ParamOut = new
                {
                    TransactionId = transactionId,
                    ReceiptNumber = receiptNumber,
                    PhoneNumber = phoneNumber,
                    BillerCode = billerConfig.BillerCode,
                    BillerName = billerConfig.BillerName,
                    Amount = amount,
                    RechargeDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = TransactionStatus.COMPLETED
                }
            };

            // Mettre à jour la transaction avec la réponse
            transaction.RawResponse = JsonSerializer.Serialize(response);
            await _transactionRepository.UpdateAsync(transaction);

            _logger.LogInfo($"Recharge simulée avec succès pour {billerConfig.BillerCode}, Téléphone: {phoneNumber}, Montant: {amount}");
            return response;
        }

        /// <summary>
        /// Recrée une réponse à partir d'une transaction existante (pour l'idempotence)
        /// </summary>
        private async Task<B3gServiceResponse> RecreateResponseFromTransaction(Transaction transaction, B3gServiceRequest request)
        {
            // Si la réponse est déjà stockée, l'utiliser
            if (!string.IsNullOrEmpty(transaction.RawResponse))
            {
                try
                {
                    var storedResponse = JsonSerializer.Deserialize<B3gServiceResponse>(transaction.RawResponse);
                    if (storedResponse != null)
                    {
                        // Mettre à jour les IDs de session et service pour la nouvelle requête
                        storedResponse.SessionId = request.SessionId;
                        storedResponse.ServiceId = request.ServiceId;
                        return storedResponse;
                    }
                }
                catch
                {
                    // En cas d'erreur de désérialisation, continuer avec la création d'une nouvelle réponse
                }
            }

            // Sinon, créer une nouvelle réponse basée sur les données de la transaction
            var response = new B3gServiceResponse
            {
                SessionId = request.SessionId,
                ServiceId = request.ServiceId,
                StatusCode = BillPaymentProvider.Core.Constants.StatusCodes.SUCCESS,
                StatusLabel = "Transaction déjà traitée"
            };

            // Déterminer le type de réponse en fonction des données disponibles
            if (!string.IsNullOrEmpty(transaction.PhoneNumber))
            {
                // C'est une recharge télécom
                response.ParamOut = new
                {
                    TransactionId = transaction.TransactionId,
                    ReceiptNumber = transaction.ReceiptNumber,
                    PhoneNumber = transaction.PhoneNumber,
                    BillerCode = transaction.BillerCode,
                    Amount = transaction.Amount,
                    RechargeDate = transaction.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = transaction.Status
                };
            }
            else
            {
                // C'est un paiement de facture
                response.ParamOut = new
                {
                    TransactionId = transaction.TransactionId,
                    ReceiptNumber = transaction.ReceiptNumber,
                    CustomerReference = transaction.CustomerReference,
                    BillerCode = transaction.BillerCode,
                    Amount = transaction.Amount,
                    PaymentDate = transaction.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = transaction.Status
                };
            }

            return response;
        }

        /// <summary>
        /// Crée une réponse d'erreur
        /// </summary>
        private B3gServiceResponse CreateErrorResponse(B3gServiceRequest request, string statusCode, string statusLabel)
        {
            var lang = GetLang(request);
            return new B3gServiceResponse
            {
                SessionId = request.SessionId,
                ServiceId = request.ServiceId,
                StatusCode = statusCode,
                StatusLabel = LocalizationHelper.GetMessage(statusCode, lang) + (statusLabel != null && statusLabel != LocalizationHelper.GetMessage(statusCode, lang) ? $" - {statusLabel}" : ""),
                ParamOut = new { ErrorMessage = statusLabel }
            };
        }

        /// <summary>
        /// Simule une erreur aléatoire
        /// </summary>
        private B3gServiceResponse SimulateRandomError(B3gServiceRequest request)
        {
            var errorType = _random.Next(1, 5);

            switch (errorType)
            {
                case 1:
                    return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.SERVICE_UNAVAILABLE, "Service temporairement indisponible");
                case 2:
                    return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.TIMEOUT, "Délai d'attente dépassé");
                case 3:
                    return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.EXTERNAL_SERVICE_ERROR, "Erreur du service externe");
                case 4:
                    return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.DATABASE_ERROR, "Erreur de base de données");
                default:
                    return CreateErrorResponse(request, BillPaymentProvider.Core.Constants.StatusCodes.SYSTEM_ERROR, "Erreur système inconnue");
            }
        }

        /// <summary>
        /// Génère un montant aléatoire
        /// </summary>
        private decimal GenerateRandomAmount(decimal min, decimal max)
        {
            var range = (double)(max - min);
            var amount = min + (decimal)(_random.NextDouble() * range);
            return Math.Round(amount, 2);
        }

        /// <summary>
        /// Génère un numéro de reçu unique
        /// </summary>
        private string GenerateReceiptNumber()
        {
            return $"REC{DateTime.UtcNow:yyyyMMdd}{_random.Next(100000, 999999)}";
        }

        /// <summary>
        /// Génère un numéro de facture aléatoire
        /// </summary>
        private string GenerateRandomBillNumber()
        {
            return $"INV{DateTime.UtcNow:yyyyMM}{_random.Next(10000, 99999)}";
        }

        /// <summary>
        /// Génère un nom de client aléatoire
        /// </summary>
        private string GenerateRandomCustomerName()
        {
            var firstNames = new[] { "Mohamed", "Ahmed", "Ali", "Omar", "Mahmoud", "Khaled", "Yousef", "Ibrahim", "Hassan", "Mostafa" };
            var lastNames = new[] { "Ibrahim", "Ahmed", "Mohamed", "Ali", "Hussein", "Hassan", "Nasser", "Saad", "Sayed", "Mahmoud" };

            return $"{firstNames[_random.Next(firstNames.Length)]} {lastNames[_random.Next(lastNames.Length)]}";
        }

        #endregion
    }
}
