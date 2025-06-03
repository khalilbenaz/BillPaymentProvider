namespace BillPaymentProvider.Core.Enums
{
    /// <summary>
    /// Catégories de créanciers
    /// </summary>
    public static class BillerType
    {
        /// <summary>
        /// Fournisseur d'électricité
        /// </summary>
        public const string ELECTRICITY = "ELECTRICITY";

        /// <summary>
        /// Fournisseur d'eau
        /// </summary>
        public const string WATER = "WATER";

        /// <summary>
        /// Fournisseur de gaz
        /// </summary>
        public const string GAS = "GAS";

        /// <summary>
        /// Fournisseur de téléphonie fixe
        /// </summary>
        public const string PHONE = "PHONE";

        /// <summary>
        /// Fournisseur internet
        /// </summary>
        public const string INTERNET = "INTERNET";

        /// <summary>
        /// Services d'abonnement
        /// </summary>
        public const string SUBSCRIPTION = "SUBSCRIPTION";

        /// <summary>
        /// Opérateur télécom mobile
        /// </summary>
        public const string TELECOM = "TELECOM";

        /// <summary>
        /// Services de transport (ex. métro, bus)
        /// </summary>
        public const string TRANSPORT = "TRANSPORT";

        /// <summary>
        /// Chaînes de télévision ou services satellite
        /// </summary>
        public const string TELEVISION = "TELEVISION";

        /// <summary>
        /// Services gouvernementaux (impôts, amendes...)
        /// </summary>
        public const string GOVERNMENT = "GOVERNMENT";
    }
}
