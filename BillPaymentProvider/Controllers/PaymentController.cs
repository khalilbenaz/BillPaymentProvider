using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BillPaymentProvider.Controllers
{
    /// <summary>
    /// Contrôleur pour tous types de paiements (factures et recharges)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Point d'entrée unique pour toutes les opérations de paiement
        /// </summary>
        /// <remarks>
        /// Cette méthode traite différentes opérations selon le paramètre "Operation" :
        /// - INQUIRE : Interrogation des informations (facture ou téléphone)
        /// - PAY : Paiement de facture ou recharge télécom
        /// - STATUS : Vérification du statut d'une transaction
        /// - CANCEL : Annulation d'une transaction
        /// 
        /// Exemple de requête pour interrogation de facture :
        /// ```json
        /// {
        ///   "SessionId": "12345678-1234-1234-1234-123456789012",
        ///   "ServiceId": "bill_payment",
        ///   "UserName": "test_user",
        ///   "Password": "test_password",
        ///   "Language": "fr",
        ///   "ChannelId": "WEB",
        ///   "IsDemo": 1,
        ///   "ParamIn": {
        ///     "Operation": "INQUIRE",
        ///     "BillerCode": "EGY-ELECTRICITY",
        ///     "CustomerReference": "123456789"
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Requête de paiement standardisée</param>
        /// <returns>Liste de réponses standardisées</returns>
        /// <response code="200">Opération réussie (vérifier le StatusCode dans la réponse)</response>
        /// <response code="400">Paramètres invalides</response>
        /// <response code="500">Erreur serveur</response>
        [HttpPost("process")]
        [ProducesResponseType(typeof(List<B3gServiceResponse>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public ActionResult<List<B3gServiceResponse>> Process([FromBody][Required] B3gServiceRequest request)
        {
            if (request == null || request.ParamIn == null)
            {
                return BadRequest("Requête invalide");
            }

            return _paymentService.Process(request);
        }
    }
}
