using System.Text.Json.Serialization;

namespace BillPaymentProvider.Core.Models
{
    /// <summary>
    /// Réponse standard de service B3G
    /// </summary>
    public class B3gServiceResponse
    {
        /// <summary>
        /// Identifiant de la session (même que dans la requête)
        /// </summary>
        [JsonPropertyName("SessionId")]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Identifiant du service (même que dans la requête)
        /// </summary>
        [JsonPropertyName("ServiceId")]
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Code de statut (000 = succès)
        /// </summary>
        [JsonPropertyName("StatusCode")]
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Libellé du statut
        /// </summary>
        [JsonPropertyName("StatusLabel")]
        public string StatusLabel { get; set; } = string.Empty;

        /// <summary>
        /// Paramètres de sortie (résultat)
        /// </summary>
        [JsonPropertyName("ParamOut")]
        public object? ParamOut { get; set; }

        /// <summary>
        /// Paramètres de sortie au format JSON (pour les systèmes legacy)
        /// </summary>
        [JsonPropertyName("ParamOutJson")]
        public object? ParamOutJson { get; set; }
    }
}
