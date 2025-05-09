using System.ComponentModel.DataAnnotations;

namespace BillPaymentProvider.Core.Models
{
    /// <summary>
    /// Journal des événements d'une transaction
    /// </summary>
    public class TransactionLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identifiant de la transaction
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Action effectuée (PAYMENT, RECHARGE, CANCEL, etc.)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Détails de l'action
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Statut précédent
        /// </summary>
        [StringLength(20)]
        public string? PreviousStatus { get; set; }

        /// <summary>
        /// Nouveau statut
        /// </summary>
        [StringLength(20)]
        public string? NewStatus { get; set; }

        /// <summary>
        /// Date et heure de l'événement
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
