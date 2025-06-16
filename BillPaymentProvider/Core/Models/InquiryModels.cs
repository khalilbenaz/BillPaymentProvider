using System.ComponentModel.DataAnnotations;

namespace BillPaymentProvider.Core.Models
{
    /// <summary>
    /// Modèle de requête pour la consultation de factures
    /// </summary>
    public class InquiryRequest
    {
        /// <summary>
        /// Code du créancier (EGY-ELECTRICITY, EGY-WATER, EGY-GAS, etc.)
        /// </summary>
        [Required(ErrorMessage = "Le champ 'BillerCode' est requis")]
        public string BillerCode { get; set; } = string.Empty;

        /// <summary>
        /// Référence client pour les factures (électricité, eau, gaz)
        /// </summary>
        public string? CustomerReference { get; set; }

        /// <summary>
        /// Numéro de téléphone pour les recharges télécom
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Type de consultation (optionnel)
        /// </summary>
        public string? InquiryType { get; set; } = "BILL_AMOUNT";
    }

    /// <summary>
    /// Modèle de réponse pour la consultation de factures
    /// </summary>
    public class InquiryResponse
    {
        /// <summary>
        /// Code de statut (000 = succès)
        /// </summary>
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Message de statut
        /// </summary>
        public string StatusMessage { get; set; } = string.Empty;

        /// <summary>
        /// Détails de la facture ou de la validation
        /// </summary>
        public InquiryDetails? Details { get; set; }
    }

    /// <summary>
    /// Détails de la consultation
    /// </summary>
    public class InquiryDetails
    {
        /// <summary>
        /// Code du créancier
        /// </summary>
        public string? BillerCode { get; set; }

        /// <summary>
        /// Nom du créancier
        /// </summary>
        public string? BillerName { get; set; }

        /// <summary>
        /// Référence client
        /// </summary>
        public string? CustomerReference { get; set; }

        /// <summary>
        /// Nom du client
        /// </summary>
        public string? CustomerName { get; set; }

        /// <summary>
        /// Montant de la facture
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Date d'échéance
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Numéro de facture
        /// </summary>
        public string? BillNumber { get; set; }

        /// <summary>
        /// Statut de la facture
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Informations supplémentaires
        /// </summary>
        public Dictionary<string, object>? AdditionalInfo { get; set; }
    }
}
