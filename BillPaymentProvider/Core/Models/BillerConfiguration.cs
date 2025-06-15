using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace BillPaymentProvider.Core.Models
{
    /// <summary>
    /// Configuration d'un créancier dans le système
    /// </summary>
    public class BillerConfiguration
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Code unique du créancier
        /// </summary>
        [Required]
        [StringLength(50)]
        public string BillerCode { get; set; } = null!;

        /// <summary>
        /// Nom du créancier
        /// </summary>
        [Required]
        [StringLength(100)]
        public string BillerName { get; set; } = null!;

        /// <summary>
        /// Description du service
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Catégorie du créancier (ELECTRICITY, WATER, GAS, TELECOM, etc.)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = null!;

        /// <summary>
        /// Type de service (BILL_PAYMENT, TELECOM_RECHARGE)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ServiceType { get; set; } = null!;

        /// <summary>
        /// Format de référence client (REGEX)
        /// </summary>
        [StringLength(200)]
        public string? CustomerReferenceFormat { get; set; }

        /// <summary>
        /// Format de numéro de téléphone pour les recharges (REGEX)
        /// </summary>
        [StringLength(200)]
        public string? PhoneNumberFormat { get; set; }

        /// <summary>
        /// Montants disponibles pour les recharges (JSON array de nombres)
        /// </summary>
        [StringLength(500)]
        public string? AvailableAmounts { get; set; }

        /// <summary>
        /// Paramètres de configuration spécifiques au créancier (JSON)
        /// </summary>
        public string? SpecificParams { get; set; }

        /// <summary>
        /// URL du service simulé externe (si applicable)
        /// </summary>
        [StringLength(200)]
        public string? ServiceUrl { get; set; }

        /// <summary>
        /// Si true, les messages d'erreur aléatoires peuvent être générés
        /// </summary>
        public bool SimulateRandomErrors { get; set; } = false;

        /// <summary>
        /// Pourcentage d'erreurs à simuler (1-100)
        /// </summary>
        public int ErrorRate { get; set; } = 0;

        /// <summary>
        /// Délai de traitement simulé en millisecondes
        /// </summary>
        public int ProcessingDelay { get; set; } = 0;

        /// <summary>
        /// Si le créancier est actif dans le système
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date de création
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Dernière mise à jour
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Logo ou image du créancier (Base64)
        /// </summary>
        public string? LogoBase64 { get; set; }

        /// <summary>
        /// Méthode utilitaire pour obtenir les montants disponibles sous forme de liste
        /// </summary>
        public List<decimal> GetAvailableAmounts()
        {
            if (string.IsNullOrEmpty(AvailableAmounts))
                return new List<decimal>();

            try
            {
                return JsonSerializer.Deserialize<List<decimal>>(AvailableAmounts) ?? new List<decimal>();
            }
            catch
            {
                return new List<decimal>();
            }
        }

        /// <summary>
        /// Méthode utilitaire pour obtenir les paramètres spécifiques sous forme de dictionnaire
        /// </summary>
        public Dictionary<string, object> GetSpecificParams()
        {
            if (string.IsNullOrEmpty(SpecificParams))
                return new Dictionary<string, object>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(SpecificParams) ??
                       new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }
    }
}
