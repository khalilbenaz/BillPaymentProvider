using System.Text.Json.Serialization;

namespace BillPaymentProvider.Core.Models
{
    /// <summary>
    /// Requête standard de service B3G
    /// </summary>
    public class B3gServiceRequest
    {
        /// <summary>
        /// Identifiant unique de la session
        /// </summary>
        [JsonPropertyName("SessionId")]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Identifiant du service demandé
        /// </summary>
        [JsonPropertyName("ServiceId")]
        public string ServiceId { get; set; } = string.Empty;

        /// <summary>
        /// Nom d'utilisateur pour l'authentification
        /// </summary>
        [JsonPropertyName("UserName")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Mot de passe pour l'authentification
        /// </summary>
        [JsonPropertyName("Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Langue des messages de retour
        /// </summary>
        [JsonPropertyName("Language")]
        public string Language { get; set; } = "fr";

        /// <summary>
        /// Canal d'origine de la demande (WEB, MOBILE, etc.)
        /// </summary>
        [JsonPropertyName("ChannelId")]
        public string? ChannelId { get; set; }

        /// <summary>
        /// Mode démo (1) ou production (0)
        /// </summary>
        [JsonPropertyName("IsDemo")]
        public int IsDemo { get; set; } = 1;

        /// <summary>
        /// Paramètres spécifiques à la requête
        /// </summary>
        [JsonPropertyName("ParamIn")]
        public Dictionary<string, object> ParamIn { get; set; } = new Dictionary<string, object>();

    }
}
