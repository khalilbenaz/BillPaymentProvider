namespace BillPaymentProvider.Core.Interfaces
{
    /// <summary>
    /// Interface pour les services de logging
    /// </summary>
    public interface IAppLogger
    {
        /// <summary>
        /// Log d'information
        /// </summary>
        void LogInfo(string message);

        /// <summary>
        /// Log d'avertissement
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// Log d'erreur
        /// </summary>
        void LogError(string message);

        /// <summary>
        /// Log de débogage
        /// </summary>
        void LogDebug(string message);

        /// <summary>
        /// Log d'information asynchrone
        /// </summary>
        Task LogInfoAsync(string message);

        /// <summary>
        /// Log d'avertissement asynchrone
        /// </summary>
        Task LogWarningAsync(string message);

        /// <summary>
        /// Log d'erreur asynchrone
        /// </summary>
        Task LogErrorAsync(string message);

        /// <summary>
        /// Log de débogage asynchrone
        /// </summary>
        Task LogDebugAsync(string message);
    }
}
