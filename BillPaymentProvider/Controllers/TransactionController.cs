using BillPaymentProvider.Core.Interfaces;
using BillPaymentProvider.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BillPaymentProvider.Controllers
{
    /// <summary>
    /// Contrôleur pour la gestion des transactions
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionController(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        /// <summary>
        /// Récupère une transaction par son ID
        /// </summary>
        /// <param name="id">Identifiant unique de la transaction</param>
        /// <returns>Détails de la transaction</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Transaction), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTransaction(string id)
        {
            var transaction = await _transactionRepository.GetByTransactionIdAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return Ok(transaction);
        }

        /// <summary>
        /// Récupère l'historique des transactions d'un client
        /// </summary>
        /// <param name="customerReference">Référence client</param>
        /// <param name="page">Numéro de page (1 par défaut)</param>
        /// <param name="pageSize">Taille de la page (20 par défaut)</param>
        /// <returns>Liste des transactions du client</returns>
        [HttpGet("history/{customerReference}")]
        [ProducesResponseType(typeof(List<Transaction>), 200)]
        public async Task<IActionResult> GetTransactionHistory(
            string customerReference,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var skip = (page - 1) * pageSize;

            var transactions = await _transactionRepository.GetTransactionHistoryAsync(
                customerReference, skip, pageSize);

            return Ok(transactions);
        }

        /// <summary>
        /// Récupère les logs d'une transaction
        /// </summary>
        /// <param name="id">Identifiant unique de la transaction</param>
        /// <returns>Liste des logs de la transaction</returns>
        [HttpGet("{id}/logs")]
        [ProducesResponseType(typeof(List<TransactionLog>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetTransactionLogs(string id)
        {
            var transaction = await _transactionRepository.GetByTransactionIdAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            var logs = await _transactionRepository.GetTransactionLogsAsync(id);

            return Ok(logs);
        }
    }
}
