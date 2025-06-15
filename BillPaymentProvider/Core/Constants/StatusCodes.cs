namespace BillPaymentProvider.Core.Constants
{
    /// <summary>
    /// Codes de statut standardisés pour les réponses API
    /// </summary>
    public static class StatusCodes
    {
        // Codes de succès
        public const string SUCCESS = "000";
        public const string PARTIAL_SUCCESS = "002";
        public const string PENDING = "001";

        // Codes d'erreur validation
        public const string INVALID_REQUEST = "100";
        public const string MISSING_PARAMETER = "101";
        public const string INVALID_PARAMETER = "102";
        public const string INVALID_AMOUNT = "103";
        public const string INVALID_REFERENCE = "104";
        public const string INVALID_PHONE = "105";

        // Codes d'erreur métier
        public const string BILLER_NOT_FOUND = "200";
        public const string SERVICE_UNAVAILABLE = "201";
        public const string BILL_NOT_FOUND = "202";
        public const string ALREADY_PAID = "203";
        public const string INSUFFICIENT_FUNDS = "204";
        public const string TRANSACTION_NOT_FOUND = "205";
        public const string CANNOT_CANCEL = "206";

        // Codes d'erreur système
        public const string SYSTEM_ERROR = "500";
        public const string DATABASE_ERROR = "501";
        public const string TIMEOUT = "502";
        public const string EXTERNAL_SERVICE_ERROR = "503";
    }
}
