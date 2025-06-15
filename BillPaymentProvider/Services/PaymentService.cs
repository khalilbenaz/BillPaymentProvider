using BillPaymentProvider.Core.Constants;
using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Providers;
using System.Text.Json;
using StatusCodes = BillPaymentProvider.Core.Constants.StatusCodes;

namespace BillPaymentProvider.Services
{
    /// <summary>
    /// Service de paiement unifié qui gère tous les types de paiement
    /// </summary>
    public class PaymentService
    {
        private readonly GenericPaymentProvider _paymentProvider;
        private readonly ITransactionRepository _transactionRepository;
        private readonly PaymentHistoryService _historyService;
        private readonly IAppLogger _logger;
        private readonly WebhookService _webhookService;

        public PaymentService(
            GenericPaymentProvider paymentProvider,
            ITransactionRepository transactionRepository,
            PaymentHistoryService historyService,
            IAppLogger logger,
            WebhookService webhookService)
        {
            _paymentProvider = paymentProvider;
            _transactionRepository = transactionRepository;
            _historyService = historyService;
            _logger = logger;
            _webhookService = webhookService;
        }

        /// <summary>
        /// Point d'entrée unique pour tous les types de paiement
        /// </summary>
        public List<B3gServiceResponse> Process(B3gServiceRequest request)
        {
            try
            {
                // Log de la requête
                _logger.LogInfo($"Requête reçue: SessionId={request.SessionId}, ServiceId={request.ServiceId}");

                // Vérifier le type d'opération demandé
                if (!request.ParamIn.TryGetValue("Operation", out var operationObj))
                {
                    var errorResponse = new B3gServiceResponse
                    {
                        SessionId = request.SessionId,
                        ServiceId = request.ServiceId,
                        StatusCode = StatusCodes.MISSING_PARAMETER,
                        StatusLabel = "Paramètre 'Operation' manquant",
                        ParamOut = null
                    };

                    return new List<B3gServiceResponse> { errorResponse };
                }

                var operation = operationObj?.ToString()?.ToUpper() ?? string.Empty;

                // Exécuter l'opération demandée
                List<B3gServiceResponse> responses;

                switch (operation)
                {
                    case "INQUIRE":
                        responses = new List<B3gServiceResponse> { _paymentProvider.InquireAsync(request).Result };
                        break;

                    case "INQUIRE_MULTIPLE":
                        responses = InquireMultipleAsync(request).Result;
                        break;

                    case "PAY":
                        responses = new List<B3gServiceResponse> { _paymentProvider.ProcessPaymentAsync(request).Result };
                        break;

                    case "PAY_MULTIPLE":
                        responses = ProcessMultiplePaymentsAsync(request).Result;
                        break;

                    case "STATUS":
                        responses = new List<B3gServiceResponse> { _paymentProvider.CheckStatusAsync(request).Result };
                        break;

                    case "CANCEL":
                        responses = new List<B3gServiceResponse> { _paymentProvider.CancelTransactionAsync(request).Result };
                        break;

                    default:
                        responses = new List<B3gServiceResponse> {
                        new B3gServiceResponse
                        {
                            SessionId = request.SessionId,
                            ServiceId = request.ServiceId,
                            StatusCode = StatusCodes.INVALID_PARAMETER,
                            StatusLabel = $"Opération non supportée: {operation}",
                            ParamOut = null
                        }
                    };
                        break;
                }

                // Log du résultat
                foreach (var response in responses)
                {
                    _logger.LogInfo($"Réponse envoyée: SessionId={response.SessionId}, StatusCode={response.StatusCode}");

                    // Enregistrer l'historique pour les opérations de paiement et recharge
                    if (operation == "PAY" || operation == "PAY_MULTIPLE")
                    {
                        _historyService.SaveAsync(request, response).Wait();
                        // Notifier le webhook (asynchrone, ne bloque pas la réponse)
                        _ = _webhookService.NotifyAsync(new {
                            SessionId = response.SessionId,
                            ServiceId = response.ServiceId,
                            StatusCode = response.StatusCode,
                            StatusLabel = response.StatusLabel,
                            ParamOut = response.ParamOut,
                            Date = DateTime.UtcNow
                        });
                    }
                }

                return responses;
            }
            catch (Exception ex)
            {
                // Log de l'erreur
                _logger.LogError($"Erreur lors du traitement: {ex.Message}");

                // Créer une réponse d'erreur
                var errorResponse = new B3gServiceResponse
                {
                    SessionId = request.SessionId,
                    ServiceId = request.ServiceId,
                    StatusCode = StatusCodes.SYSTEM_ERROR,
                    StatusLabel = $"Erreur interne: {ex.Message}",
                    ParamOut = null
                };

                return new List<B3gServiceResponse> { errorResponse };
            }
        }

        /// <summary>
        /// Traite une requête pour consulter plusieurs factures à la fois
        /// </summary>
        private async Task<List<B3gServiceResponse>> InquireMultipleAsync(B3gServiceRequest request)
        {
            // Vérifier les paramètres requis
            if (!request.ParamIn.TryGetValue("BillerCode", out var billerCodeObj))
            {
                return new List<B3gServiceResponse> {
                new B3gServiceResponse
                {
                    SessionId = request.SessionId,
                    ServiceId = request.ServiceId,
                    StatusCode = StatusCodes.MISSING_PARAMETER,
                    StatusLabel = "BillerCode manquant",
                    ParamOut = null
                }
            };
            }

            if (!request.ParamIn.TryGetValue("CustomerReference", out var customerRefObj))
            {
                return new List<B3gServiceResponse> {
                new B3gServiceResponse
                {
                    SessionId = request.SessionId,
                    ServiceId = request.ServiceId,
                    StatusCode = StatusCodes.MISSING_PARAMETER,
                    StatusLabel = "CustomerReference manquant",
                    ParamOut = null
                }
            };
            }

            // Créer une requête de consultation standard
            var inquireRequest = new B3gServiceRequest
            {
                SessionId = request.SessionId,
                ServiceId = request.ServiceId,
                UserName = request.UserName,
                Password = request.Password,
                Language = request.Language,
                ChannelId = request.ChannelId,
                IsDemo = request.IsDemo,
                ParamIn = new Dictionary<string, object>
            {
                { "Operation", "INQUIRE" },
                { "BillerCode", billerCodeObj },
                { "CustomerReference", customerRefObj }
            }
            };

            // Appeler le provider pour récupérer une facture
            var response = await _paymentProvider.InquireAsync(inquireRequest);

            if (response.StatusCode != StatusCodes.SUCCESS)
            {
                // En cas d'erreur, retourner directement la réponse
                return new List<B3gServiceResponse> { response };
            }

            // Simuler plusieurs factures pour la même référence client
            var bills = new List<object>();

            // La première facture est celle renvoyée par le provider
            if (response.ParamOut != null)
            {
                bills.Add(response.ParamOut);
            }

            // Simuler 1 à 3 factures supplémentaires
            var random = new Random();
            var numberOfAdditionalBills = random.Next(1, 4);

            for (int i = 1; i <= numberOfAdditionalBills; i++)
            {
                // Générer des données aléatoires pour cette facture
                var dueAmount = Math.Round(random.NextDouble() * 500, 2);
                var dueDate = DateTime.UtcNow.AddDays(random.Next(5, 30));
                var billPeriod = $"{DateTime.UtcNow.AddMonths(-i):MMM yyyy}";
                var billNumber = $"INV{DateTime.UtcNow.AddMonths(-i):yyyyMM}{random.Next(10000, 99999)}";

                // Extraire les propriétés communes de la première facture
                var paramOut = response.ParamOut as dynamic;
                var billerCode = paramOut?.BillerCode?.ToString() ?? billerCodeObj.ToString();
                var billerName = paramOut?.BillerName?.ToString() ?? "Fournisseur";
                var customerReference = paramOut?.CustomerReference?.ToString() ?? customerRefObj.ToString();
                var customerName = paramOut?.CustomerName?.ToString() ?? "Client";

                // Créer une nouvelle facture
                var bill = new
                {
                    BillerCode = billerCode,
                    BillerName = billerName,
                    CustomerReference = customerReference,
                    CustomerName = customerName,
                    DueAmount = dueAmount,
                    DueDate = dueDate.ToString("yyyy-MM-dd"),
                    BillPeriod = billPeriod,
                    BillNumber = billNumber,
                    BillType = i == 1 ? "Current" : "Past",
                    BillIndex = i
                };

                bills.Add(bill);
            }

            // Créer une réponse globale contenant toutes les factures
            var multipleResponse = new B3gServiceResponse
            {
                SessionId = request.SessionId,
                ServiceId = request.ServiceId,
                StatusCode = StatusCodes.SUCCESS,
                StatusLabel = $"Factures trouvées ({bills.Count})",
                ParamOut = new
                {
                    BillerCode = billerCodeObj.ToString(),
                    CustomerReference = customerRefObj.ToString(),
                    BillCount = bills.Count,
                    Bills = bills
                }
            };

            return new List<B3gServiceResponse> { multipleResponse };
        }

        /// <summary>
        /// Traite une requête pour payer plusieurs factures à la fois
        /// </summary>
        private async Task<List<B3gServiceResponse>> ProcessMultiplePaymentsAsync(B3gServiceRequest request)
        {
            // Vérifier les paramètres requis
            if (!request.ParamIn.TryGetValue("Payments", out var paymentsObj))
            {
                return new List<B3gServiceResponse> {
                new B3gServiceResponse
                {
                    SessionId = request.SessionId,
                    ServiceId = request.ServiceId,
                    StatusCode = StatusCodes.MISSING_PARAMETER,
                    StatusLabel = "Paramètre 'Payments' manquant",
                    ParamOut = null
                }
            };
            }

            List<Dictionary<string, object>> payments;

            try
            {
                // Convertir la liste des paiements
                if (paymentsObj is string paymentsJson)
                {
                    payments = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(paymentsJson)
                        ?? new List<Dictionary<string, object>>();
                }
                else if (paymentsObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                {
                    payments = jsonElement.EnumerateArray()
                        .Select(item => item.Deserialize<Dictionary<string, object>>() ?? new Dictionary<string, object>())
                        .ToList();
                }
                else if (paymentsObj is List<object> objList)
                {
                    payments = objList
                        .Select(item => item as Dictionary<string, object> ?? new Dictionary<string, object>())
                        .ToList();
                }
                else
                {
                    return new List<B3gServiceResponse> {
                    new B3gServiceResponse
                    {
                        SessionId = request.SessionId,
                        ServiceId = request.ServiceId,
                        StatusCode = StatusCodes.INVALID_PARAMETER,
                        StatusLabel = "Format de 'Payments' invalide",
                        ParamOut = null
                    }
                };
                }
            }
            catch (Exception ex)
            {
                return new List<B3gServiceResponse> {
                new B3gServiceResponse
                {
                    SessionId = request.SessionId,
                    ServiceId = request.ServiceId,
                    StatusCode = StatusCodes.INVALID_PARAMETER,
                    StatusLabel = $"Erreur de désérialisation: {ex.Message}",
                    ParamOut = null
                }
            };
            }

            if (payments.Count == 0)
            {
                return new List<B3gServiceResponse> {
                new B3gServiceResponse
                {
                    SessionId = request.SessionId,
                    ServiceId = request.ServiceId,
                    StatusCode = StatusCodes.INVALID_PARAMETER,
                    StatusLabel = "Aucun paiement spécifié",
                    ParamOut = null
                }
            };
            }

            // Traiter chaque paiement individuellement
            var responses = new List<B3gServiceResponse>();
            var globalTransactionId = Guid.NewGuid().ToString("N"); // ID commun pour lier les transactions

            foreach (var payment in payments)
            {
                // Vérifier les paramètres requis pour chaque paiement
                if (!payment.TryGetValue("BillerCode", out var billerCode) ||
                    !payment.TryGetValue("Amount", out var amount))
                {
                    responses.Add(new B3gServiceResponse
                    {
                        SessionId = request.SessionId,
                        ServiceId = request.ServiceId,
                        StatusCode = StatusCodes.MISSING_PARAMETER,
                        StatusLabel = "Paramètres 'BillerCode' ou 'Amount' manquants dans un paiement",
                        ParamOut = new
                        {
                            Payment = payment,
                            Error = "Paramètres incomplets"
                        }
                    });

                    continue;
                }

                // Vérifier si c'est une facture ou une recharge
                bool isRecharge = payment.ContainsKey("PhoneNumber");

                if (!isRecharge && !payment.ContainsKey("CustomerReference"))
                {
                    responses.Add(new B3gServiceResponse
                    {
                        SessionId = request.SessionId,
                        ServiceId = request.ServiceId,
                        StatusCode = StatusCodes.MISSING_PARAMETER,
                        StatusLabel = "Paramètre 'CustomerReference' ou 'PhoneNumber' manquant",
                        ParamOut = new
                        {
                            Payment = payment,
                            Error = "Référence client ou numéro de téléphone requis"
                        }
                    });

                    continue;
                }

                // Créer une requête de paiement standard pour ce paiement
                var paymentRequest = new B3gServiceRequest
                {
                    SessionId = Guid.NewGuid().ToString(), // Unique pour chaque paiement
                    ServiceId = request.ServiceId,
                    UserName = request.UserName,
                    Password = request.Password,
                    Language = request.Language,
                    ChannelId = request.ChannelId,
                    IsDemo = request.IsDemo,
                    ParamIn = new Dictionary<string, object>
                {
                    { "Operation", "PAY" },
                    { "BillerCode", billerCode },
                    { "Amount", amount },
                    { "GroupTransactionId", globalTransactionId } // Pour lier les transactions
                }
                };

                // Ajouter CustomerReference ou PhoneNumber selon le cas
                if (isRecharge)
                {
                    paymentRequest.ParamIn.Add("PhoneNumber", payment["PhoneNumber"]);
                }
                else
                {
                    paymentRequest.ParamIn.Add("CustomerReference", payment["CustomerReference"]);
                }

                // Mode sandbox : simulation de paiement
                if (request.IsDemo == 1)
                {
                    var simulatedResponse = new B3gServiceResponse
                    {
                        SessionId = paymentRequest.SessionId,
                        ServiceId = paymentRequest.ServiceId,
                        StatusCode = StatusCodes.SUCCESS,
                        StatusLabel = "Paiement simulé (sandbox)",
                        ParamOut = new
                        {
                            TransactionId = Guid.NewGuid().ToString(),
                            Payment = payment,
                            Simulated = true,
                            Message = "Aucune opération réelle effectuée. Ceci est une simulation sandbox."
                        }
                    };
                    responses.Add(simulatedResponse);
                    continue;
                }

                // Ajouter les autres paramètres spécifiques
                foreach (var param in payment)
                {
                    if (param.Key != "BillerCode" && param.Key != "Amount" &&
                        param.Key != "CustomerReference" && param.Key != "PhoneNumber" &&
                        !paymentRequest.ParamIn.ContainsKey(param.Key))
                    {
                        paymentRequest.ParamIn.Add(param.Key, param.Value);
                    }
                }

                // Traiter ce paiement
                var response = await _paymentProvider.ProcessPaymentAsync(paymentRequest);
                responses.Add(response);
            }

            // Créer une réponse globale résumant tous les paiements
            var successCount = responses.Count(r => r.StatusCode == StatusCodes.SUCCESS);
            var failedCount = responses.Count - successCount;

            var summary = new B3gServiceResponse
            {
                SessionId = request.SessionId,
                ServiceId = request.ServiceId,
                StatusCode = failedCount == 0 ? StatusCodes.SUCCESS : StatusCodes.PARTIAL_SUCCESS,
                StatusLabel = $"Paiements traités: {successCount} réussis, {failedCount} échoués",
                ParamOut = new
                {
                    TotalPayments = responses.Count,
                    SuccessCount = successCount,
                    FailedCount = failedCount,
                    GlobalTransactionId = globalTransactionId,
                    Details = responses.Select(r => new
                    {
                        StatusCode = r.StatusCode,
                        StatusLabel = r.StatusLabel,
                        Details = r.ParamOut
                    }).ToList()
                }
            };

            // Ajouter le résumé au début de la liste
            responses.Insert(0, summary);

            return responses;
        }
    }
}
