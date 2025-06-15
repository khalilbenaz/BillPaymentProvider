namespace BillPaymentProvider.Core.Enums
{
    /// <summary>
    /// Statuts possibles d'une transaction
    /// </summary>
    public static class TransactionStatus
    {
        /// <summary>
        /// Transaction en attente
        /// </summary>
        public const string PENDING = "PENDING";

        /// <summary>
        /// Transaction terminée avec succès
        /// </summary>
        public const string COMPLETED = "COMPLETED";

        /// <summary>
        /// Transaction échouée
        /// </summary>
        public const string FAILED = "FAILED";

        /// <summary>
        /// Transaction annulée
        /// </summary>
        public const string CANCELLED = "CANCELLED";

        /// <summary>
        /// Transaction en cours de remboursement
        /// </summary>
        public const string REFUNDING = "REFUNDING";

        /// <summary>
        /// Transaction remboursée
        /// </summary>
        public const string REFUNDED = "REFUNDED";
    }
}
