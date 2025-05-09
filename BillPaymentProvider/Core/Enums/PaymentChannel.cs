namespace BillPaymentProvider.Core.Enums
{
    /// <summary>
    /// Canaux de paiement supportés
    /// </summary>
    public static class PaymentChannel
    {
        /// <summary>
        /// Interface web
        /// </summary>
        public const string WEB = "WEB";

        /// <summary>
        /// Application mobile
        /// </summary>
        public const string MOBILE = "MOBILE";

        /// <summary>
        /// Paiement en espèces
        /// </summary>
        public const string CASH = "CASH";

        /// <summary>
        /// Kiosque de paiement
        /// </summary>
        public const string KIOSK = "KIOSK";

        /// <summary>
        /// Point de vente
        /// </summary>
        public const string POS = "POS";

        /// <summary>
        /// API externe
        /// </summary>
        public const string API = "API";
    }
}
