using System.ComponentModel.DataAnnotations;

namespace BillPaymentProvider.Core.Models
{
    /// <summary>
    /// Entrée de log dans la base de données
    /// </summary>
    public class LogEntry
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Message de log
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Niveau de log (Debug, Info, Warning, Error)
        /// </summary>
        [Required]
        public LogLevel Level { get; set; }

        /// <summary>
        /// Représentation textuelle du niveau
        /// </summary>
        [Required]
        [StringLength(10)]
        public string LevelString => Level.ToString();

        /// <summary>
        /// Date et heure du log
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
