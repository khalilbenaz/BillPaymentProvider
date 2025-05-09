using System.ComponentModel.DataAnnotations;

namespace BillPaymentProvider.Core.Models
{
    /// <summary>
    /// Représente une transaction de paiement ou de recharge
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Identifiant auto-incrémenté en base de données
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identifiant unique de la transaction (pour l'idempotence)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Code du créancier
        /// </summary>
        [Required]
        [StringLength(50)]
        public string BillerCode { get; set; } = string.Empty;

        /// <summary>
        /// Code de l'opérateur (pour les recharges télécom)
        /// </summary>
        [StringLength(50)]
        public string? OperatorCode { get; set; }

        /// <summary>
        /// Référence client (pour les paiements de factures)
        /// </summary>
        [StringLength(50)]
        public string? CustomerReference { get; set; }

        /// <summary>
        /// Numéro de téléphone (pour les recharges télécom)
        /// </summary>
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Montant de la transaction
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// Statut de la transaction (PENDING, COMPLETED, FAILED, CANCELLED)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Identifiant de session
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Identifiant du service
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Canal de paiement (WEB, MOBILE, CASH, etc.)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// Raison de l'échec (si applicable)
        /// </summary>
        [StringLength(500)]
        public string? FailureReason { get; set; }

        /// <summary>
        /// Numéro de reçu (pour les transactions réussies)
        /// </summary>
        [StringLength(50)]
        public string? ReceiptNumber { get; set; }

        /// <summary>
        /// Date de création de la transaction
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date de complétion de la transaction (si applicable)
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Date d'annulation de la transaction (si applicable)
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// Requête brute (pour debugging et idempotence)
        /// </summary>
        public string? RawRequest { get; set; }

        /// <summary>
        /// Réponse brute (pour debugging et idempotence)
        /// </summary>
        public string? RawResponse { get; set; }
    }
}
