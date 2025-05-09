using BillPaymentProvider.Core.Enums;
using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BillPaymentProvider.Utils
{
    /// <summary>
    /// Classe d'initialisation de la base de données (sans scripts SQL externes)
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Initialise la base de données avec les tables et données initiales
        /// </summary>
        public static void Initialize(AppDbContext context, IAppLogger logger)
        {
            try
            {
                // Vérifier si l'application a été déjà migrée (auto-migration)
                if (context.Database.GetPendingMigrations().Any())
                {
                    logger.LogInfo("Application des migrations en attente...");
                    context.Database.Migrate();
                }

                // S'assurer que la base de données est créée avec le schéma actuel
                context.Database.EnsureCreated();

                // Vérifier si des données existent déjà
                if (context.BillerConfigurations.Any())
                {
                    logger.LogInfo("Base de données déjà initialisée.");
                    return;
                }

                logger.LogInfo("Création des tables et insertion des données initiales...");

                // Ajouter les configurations de facturiers
                CreateBillerConfigurations(context);

                // Ajouter des exemples de transactions
                CreateSampleTransactions(context);

                // Ajouter des logs de transactions
                CreateTransactionLogs(context);

                // Ajouter l'historique des paiements
                CreatePaymentHistories(context);

                // Ajouter quelques logs système
                CreateSystemLogs(context);

                logger.LogInfo("Base de données initialisée avec succès.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Erreur lors de l'initialisation de la base de données: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Crée les configurations des créanciers
        /// </summary>
        private static void CreateBillerConfigurations(AppDbContext context)
        {
            var billers = new List<BillerConfiguration>
        {
            // Facturiers pour paiement de factures
            new BillerConfiguration
            {
                BillerCode = "EGY-ELECTRICITY",
                BillerName = "Électricité d'Égypte",
                Description = "Paiement des factures d'électricité",
                Category = BillerType.ELECTRICITY,
                ServiceType = ServiceType.BILL_PAYMENT,
                CustomerReferenceFormat = "^[0-9]{8,12}$",
                SpecificParams = JsonSerializer.Serialize(new
                {
                    fixedFee = 1.5,
                    paymentDays = "1-25"
                }),
                SimulateRandomErrors = true,
                ErrorRate = 5,
                ProcessingDelay = 500,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BillerConfiguration
            {
                BillerCode = "EGY-WATER",
                BillerName = "Compagnie des Eaux d'Égypte",
                Description = "Paiement des factures d'eau",
                Category = BillerType.WATER,
                ServiceType = ServiceType.BILL_PAYMENT,
                CustomerReferenceFormat = "^[A-Z]{2}[0-9]{6,10}$",
                SpecificParams = JsonSerializer.Serialize(new
                {
                    fixedFee = 1.0,
                    paymentDays = "1-28"
                }),
                SimulateRandomErrors = true,
                ErrorRate = 8,
                ProcessingDelay = 700,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BillerConfiguration
            {
                BillerCode = "EGY-GAS",
                BillerName = "Gaz d'Égypte",
                Description = "Paiement des factures de gaz",
                Category = BillerType.GAS,
                ServiceType = ServiceType.BILL_PAYMENT,
                CustomerReferenceFormat = "^GZ[0-9]{7,11}$",
                SpecificParams = JsonSerializer.Serialize(new
                {
                    fixedFee = 1.2,
                    paymentDays = "1-20"
                }),
                SimulateRandomErrors = true,
                ErrorRate = 5,
                ProcessingDelay = 600,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BillerConfiguration
            {
                BillerCode = "EGY-TELECOM",
                BillerName = "Télécom Égypte",
                Description = "Paiement des factures de téléphone fixe",
                Category = BillerType.PHONE,
                ServiceType = ServiceType.BILL_PAYMENT,
                CustomerReferenceFormat = "^TEL[0-9]{8,10}$",
                SpecificParams = JsonSerializer.Serialize(new
                {
                    fixedFee = 0.5,
                    paymentDays = "1-30"
                }),
                SimulateRandomErrors = true,
                ErrorRate = 3,
                ProcessingDelay = 300,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BillerConfiguration
            {
                BillerCode = "EGY-INTERNET",
                BillerName = "Internet Égypte",
                Description = "Paiement des factures internet",
                Category = BillerType.INTERNET,
                ServiceType = ServiceType.BILL_PAYMENT,
                CustomerReferenceFormat = "^NET[0-9]{7,9}$",
                SpecificParams = JsonSerializer.Serialize(new
                {
                    fixedFee = 0.8,
                    paymentDays = "1-25"
                }),
                SimulateRandomErrors = true,
                ErrorRate = 4,
                ProcessingDelay = 400,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            
            // Facturiers pour recharges télécom
            new BillerConfiguration
            {
                BillerCode = "EGY-ORANGE",
                BillerName = "Orange Égypte",
                Description = "Recharge téléphonique Orange",
                Category = BillerType.TELECOM,
                ServiceType = ServiceType.TELECOM_RECHARGE,
                PhoneNumberFormat = "^(010|012)[0-9]{8}$",
                AvailableAmounts = JsonSerializer.Serialize(new List<decimal> { 10, 20, 50, 100, 200, 500 }),
                SpecificParams = JsonSerializer.Serialize(new
                {
                    prefixes = new[] { "010", "012" }
                }),
                SimulateRandomErrors = true,
                ErrorRate = 3,
                ProcessingDelay = 200,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BillerConfiguration
            {
                BillerCode = "EGY-VODAFONE",
                BillerName = "Vodafone Égypte",
                Description = "Recharge téléphonique Vodafone",
                Category = BillerType.TELECOM,
                ServiceType = ServiceType.TELECOM_RECHARGE,
                PhoneNumberFormat = "^(010|011)[0-9]{8}$",
                AvailableAmounts = JsonSerializer.Serialize(new List<decimal> { 10, 25, 50, 100, 200, 500 }),
                SpecificParams = JsonSerializer.Serialize(new
                {
                    prefixes = new[] { "010", "011" }
                }),
                SimulateRandomErrors = true,
                ErrorRate = 4,
                ProcessingDelay = 250,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BillerConfiguration
            {
                BillerCode = "EGY-ETISALAT",
                BillerName = "Etisalat Égypte",
                Description = "Recharge téléphonique Etisalat",
                Category = BillerType.TELECOM,
                ServiceType = ServiceType.TELECOM_RECHARGE,
                PhoneNumberFormat = "^(011|015)[0-9]{8}$",
                AvailableAmounts = JsonSerializer.Serialize(new List<decimal> { 10, 20, 30, 50, 100, 200 }),
                SpecificParams = JsonSerializer.Serialize(new
                {
                    prefixes = new[] { "011", "015" }
                }),
                SimulateRandomErrors = true,
                ErrorRate = 5,
                ProcessingDelay = 300,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new BillerConfiguration
            {
                BillerCode = "EGY-WE",
                BillerName = "WE Égypte",
                Description = "Recharge téléphonique WE",
                Category = BillerType.TELECOM,
                ServiceType = ServiceType.TELECOM_RECHARGE,
                PhoneNumberFormat = "^(015)[0-9]{8}$",
                AvailableAmounts = JsonSerializer.Serialize(new List<decimal> { 10, 25, 50, 75, 100, 150, 200 }),
                SpecificParams = JsonSerializer.Serialize(new
                {
                    prefixes = new[] { "015" }
                }),
                SimulateRandomErrors = true,
                ErrorRate = 4,
                ProcessingDelay = 200,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

            context.BillerConfigurations.AddRange(billers);
            context.SaveChanges();
        }

        /// <summary>
        /// Crée quelques transactions d'exemple
        /// </summary>
        private static void CreateSampleTransactions(AppDbContext context)
        {
            var transactions = new List<Transaction>
        {
            // Exemple de paiement de facture d'électricité
            new Transaction
            {
                TransactionId = "9876543210abcdef",
                BillerCode = "EGY-ELECTRICITY",
                CustomerReference = "123456789",
                Amount = 150.75M,
                Status = TransactionStatus.COMPLETED,
                SessionId = "SESSION-001",
                ServiceId = "bill_payment",
                ChannelId = PaymentChannel.WEB,
                ReceiptNumber = "REC202505150001",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                CompletedAt = DateTime.UtcNow.AddHours(-2)
            },
            
            // Exemple de recharge télécom
            new Transaction
            {
                TransactionId = "abcdef9876543210",
                BillerCode = "EGY-ORANGE",
                PhoneNumber = "0101234567",
                Amount = 100.00M,
                Status = TransactionStatus.COMPLETED,
                SessionId = "SESSION-002",
                ServiceId = "telecom_recharge",
                ChannelId = PaymentChannel.MOBILE,
                ReceiptNumber = "REC202505150002",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                CompletedAt = DateTime.UtcNow.AddHours(-1)
            },
            
            // Exemple de transaction échouée
            new Transaction
            {
                TransactionId = "1234567890abcdef",
                BillerCode = "EGY-WATER",
                CustomerReference = "AB123456",
                Amount = 75.50M,
                Status = TransactionStatus.FAILED,
                SessionId = "SESSION-003",
                ServiceId = "bill_payment",
                ChannelId = PaymentChannel.WEB,
                FailureReason = "Service indisponible",
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            },
            
            // Exemple de transaction annulée
            new Transaction
            {
                TransactionId = "fedcba0987654321",
                BillerCode = "EGY-VODAFONE",
                PhoneNumber = "0111234567",
                Amount = 50.00M,
                Status = TransactionStatus.CANCELLED,
                SessionId = "SESSION-004",
                ServiceId = "telecom_recharge",
                ChannelId = PaymentChannel.WEB,
                ReceiptNumber = "REC202505150003",
                CreatedAt = DateTime.UtcNow.AddMinutes(-45),
                CompletedAt = DateTime.UtcNow.AddMinutes(-43),
                CancelledAt = DateTime.UtcNow.AddMinutes(-40)
            }
        };

            context.Transactions.AddRange(transactions);
            context.SaveChanges();
        }

        /// <summary>
        /// Crée des logs de transactions
        /// </summary>
        private static void CreateTransactionLogs(AppDbContext context)
        {
            var logs = new List<TransactionLog>
        {
            new TransactionLog
            {
                TransactionId = "9876543210abcdef",
                Action = "PAYMENT",
                Details = "Paiement EGY-ELECTRICITY pour 123456789 montant 150.75",
                NewStatus = TransactionStatus.COMPLETED,
                Timestamp = DateTime.UtcNow.AddHours(-2)
            },
            new TransactionLog
            {
                TransactionId = "abcdef9876543210",
                Action = "RECHARGE",
                Details = "Recharge EGY-ORANGE pour 0101234567 montant 100.00",
                NewStatus = TransactionStatus.COMPLETED,
                Timestamp = DateTime.UtcNow.AddHours(-1)
            },
            new TransactionLog
            {
                TransactionId = "1234567890abcdef",
                Action = "PAYMENT",
                Details = "Paiement EGY-WATER pour AB123456 montant 75.50",
                NewStatus = TransactionStatus.PENDING,
                Timestamp = DateTime.UtcNow.AddMinutes(-32)
            },
            new TransactionLog
            {
                TransactionId = "1234567890abcdef",
                Action = "ERROR",
                Details = "Échec du paiement: Service indisponible",
                PreviousStatus = TransactionStatus.PENDING,
                NewStatus = TransactionStatus.FAILED,
                Timestamp = DateTime.UtcNow.AddMinutes(-30)
            },
            new TransactionLog
            {
                TransactionId = "fedcba0987654321",
                Action = "RECHARGE",
                Details = "Recharge EGY-VODAFONE pour 0111234567 montant 50.00",
                NewStatus = TransactionStatus.COMPLETED,
                Timestamp = DateTime.UtcNow.AddMinutes(-43)
            },
            new TransactionLog
            {
                TransactionId = "fedcba0987654321",
                Action = "CANCEL",
                Details = "Annulation de la transaction",
                PreviousStatus = TransactionStatus.COMPLETED,
                NewStatus = TransactionStatus.CANCELLED,
                Timestamp = DateTime.UtcNow.AddMinutes(-40)
            }
        };

            context.TransactionLogs.AddRange(logs);
            context.SaveChanges();
        }

        /// <summary>
        /// Crée des historiques de paiement
        /// </summary>
        private static void CreatePaymentHistories(AppDbContext context)
        {
            var histories = new List<PaymentHistory>
        {
            new PaymentHistory
            {
                SessionId = "SESSION-001",
                ServiceId = "bill_payment",
                BillerCode = "EGY-ELECTRICITY",
                CustomerReference = "123456789",
                Amount = 150.75M,
                StatusCode = "000",
                ChannelId = PaymentChannel.WEB,
                TransactionId = "9876543210abcdef",
                Timestamp = DateTime.UtcNow.AddHours(-2)
            },
            new PaymentHistory
            {
                SessionId = "SESSION-002",
                ServiceId = "telecom_recharge",
                BillerCode = "EGY-ORANGE",
                PhoneNumber = "0101234567",
                Amount = 100.00M,
                StatusCode = "000",
                ChannelId = PaymentChannel.MOBILE,
                TransactionId = "abcdef9876543210",
                Timestamp = DateTime.UtcNow.AddHours(-1)
            },
            new PaymentHistory
            {
                SessionId = "SESSION-003",
                ServiceId = "bill_payment",
                BillerCode = "EGY-WATER",
                CustomerReference = "AB123456",
                Amount = 75.50M,
                StatusCode = "500",
                ChannelId = PaymentChannel.WEB,
                TransactionId = "1234567890abcdef",
                Timestamp = DateTime.UtcNow.AddMinutes(-30)
            }
        };

            context.PaymentHistories.AddRange(histories);
            context.SaveChanges();
        }

        /// <summary>
        /// Crée des logs système
        /// </summary>
        private static void CreateSystemLogs(AppDbContext context)
        {
            var logs = new List<LogEntry>
        {
            new LogEntry
            {
                Message = "Application démarrée",
                Level = Microsoft.Extensions.Logging.LogLevel.Information,
                Timestamp = DateTime.UtcNow.AddHours(-3)
            },
            new LogEntry
            {
                Message = "Paiement effectué avec succès: TransactionId=9876543210abcdef",
                Level = Microsoft.Extensions.Logging.LogLevel.Information,
                Timestamp = DateTime.UtcNow.AddHours(-2)
            },
            new LogEntry
            {
                Message = "Erreur lors du paiement: Service indisponible",
                Level = Microsoft.Extensions.Logging.LogLevel.Error,
                Timestamp = DateTime.UtcNow.AddMinutes(-30)
            }
        };

            context.LogEntries.AddRange(logs);
            context.SaveChanges();
        }
    }
}
