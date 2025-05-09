using BillPaymentProvider.Core.Enums;
using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Data;
using System.Text.Json;

namespace BillPaymentProvider.Utils
{
    /// <summary>
    /// Classe utilitaire pour initialiser les données de test
    /// </summary>
    public class DataSeeder
    {
        private readonly AppDbContext _dbContext;

        public DataSeeder(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Initialise les données si la base est vide
        /// </summary>
        public void SeedData()
        {
            // Vérifier si des données existent déjà
            if (_dbContext.BillerConfigurations.Any())
            {
                return; // La base est déjà initialisée
            }

            // Créer les configurations de facturiers
            SeedBillerConfigurations();

            // Créer quelques transactions de test
            SeedTransactions();
        }

        /// <summary>
        /// Crée les configurations de facturiers
        /// </summary>
        private void SeedBillerConfigurations()
        {
            // Facturiers pour paiement de factures
            _dbContext.BillerConfigurations.AddRange(
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
                }
            );

            // Facturiers pour recharges télécom
            _dbContext.BillerConfigurations.AddRange(
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
            );

            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Crée quelques transactions de test
        /// </summary>
        private void SeedTransactions()
        {
            // Créer des transactions de test
            var completedElecTransaction = new Transaction
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
            };

            var completedTelecomTransaction = new Transaction
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
            };

            var failedTransaction = new Transaction
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
            };

            var cancelledTransaction = new Transaction
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
            };

            _dbContext.Transactions.AddRange(
                completedElecTransaction,
                completedTelecomTransaction,
                failedTransaction,
                cancelledTransaction
            );

            // Créer des logs de transaction
            _dbContext.TransactionLogs.AddRange(
                new TransactionLog
                {
                    TransactionId = completedElecTransaction.TransactionId,
                    Action = "PAYMENT",
                    Details = $"Paiement EGY-ELECTRICITY pour 123456789 montant 150.75",
                    NewStatus = TransactionStatus.COMPLETED,
                    Timestamp = completedElecTransaction.CreatedAt
                },
                new TransactionLog
                {
                    TransactionId = completedTelecomTransaction.TransactionId,
                    Action = "RECHARGE",
                    Details = $"Recharge EGY-ORANGE pour 0101234567 montant 100.00",
                    NewStatus = TransactionStatus.COMPLETED,
                    Timestamp = completedTelecomTransaction.CreatedAt
                },
                new TransactionLog
                {
                    TransactionId = failedTransaction.TransactionId,
                    Action = "PAYMENT",
                    Details = $"Paiement EGY-WATER pour AB123456 montant 75.50",
                    NewStatus = TransactionStatus.PENDING,
                    Timestamp = failedTransaction.CreatedAt.AddMinutes(-2)
                },
                new TransactionLog
                {
                    TransactionId = failedTransaction.TransactionId,
                    Action = "ERROR",
                    Details = $"Échec du paiement: Service indisponible",
                    PreviousStatus = TransactionStatus.PENDING,
                    NewStatus = TransactionStatus.FAILED,
                    Timestamp = failedTransaction.CreatedAt
                },
                new TransactionLog
                {
                    TransactionId = cancelledTransaction.TransactionId,
                    Action = "RECHARGE",
                    Details = $"Recharge EGY-VODAFONE pour 0111234567 montant 50.00",
                    NewStatus = TransactionStatus.COMPLETED,
                    Timestamp = cancelledTransaction.CompletedAt.Value
                },
                new TransactionLog
                {
                    TransactionId = cancelledTransaction.TransactionId,
                    Action = "CANCEL",
                    Details = $"Annulation de la transaction",
                    PreviousStatus = TransactionStatus.COMPLETED,
                    NewStatus = TransactionStatus.CANCELLED,
                    Timestamp = cancelledTransaction.CancelledAt.Value
                }
            );

            _dbContext.SaveChanges();
        }
    }
}
