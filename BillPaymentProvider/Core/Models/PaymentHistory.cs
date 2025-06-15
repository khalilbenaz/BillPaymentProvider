using System.ComponentModel.DataAnnotations;

namespace BillPaymentProvider.Core.Models
{
    /// <summary>
    /// Historique des paiements pour la traçabilité
    /// </summary>
    public class PaymentHistory
    {
        [Key]
        public int Id { get; set; }

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
        /// Code du créancier
        /// </summary>
        [Required]
        [StringLength(50)]
        public string BillerCode { get; set; } = string.Empty;

        /// <summary>
        /// Référence client
        /// </summary>
        [StringLength(50)]
        public string? CustomerReference { get; set; }

        /// <summary>
        /// Numéro de téléphone (pour les recharges)
        /// </summary>
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Montant du paiement
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// Code de statut
        /// </summary>
        [Required]
        [StringLength(20)]
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Canal de paiement
        /// </summary>
        [StringLength(20)]
        public string? ChannelId { get; set; }

        /// <summary>
        /// Identifiant de transaction
        /// </summary>
        [StringLength(50)]
        public string? TransactionId { get; set; }

        /// <summary>
        /// Date de l'opération
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
