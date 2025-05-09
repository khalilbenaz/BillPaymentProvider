namespace BillPaymentProvider.Core.Enums
{
    /// <summary>
    /// Types de services supportés par l'API
    /// </summary>
    public static class ServiceType
    {
        /// <summary>
        /// Paiement de facture
        /// </summary>
        public const string BILL_PAYMENT = "BILL_PAYMENT";

        /// <summary>
        /// Recharge télécom
        /// </summary>
        public const string TELECOM_RECHARGE = "TELECOM_RECHARGE";
    }
}
