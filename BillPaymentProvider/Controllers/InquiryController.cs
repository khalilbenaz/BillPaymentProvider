using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Services;
using BillPaymentProvider.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace BillPaymentProvider.Controllers
{
    /// <summary>
    /// Contrôleur pour la consultation des factures et la validation des numéros de téléphone
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class InquiryController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly AuditLogger _auditLogger;

        public InquiryController(PaymentService paymentService, AuditLogger auditLogger)
        {
            _paymentService = paymentService;
            _auditLogger = auditLogger;
        }        /// <summary>
        /// Consulte les détails d'une facture ou valide un numéro de téléphone
        /// </summary>
        /// <remarks>
        /// Cette méthode permet d'interroger les informations d'une facture ou de valider un numéro 
        /// de téléphone avant de procéder au paiement ou à la recharge.
        /// 
        /// Exemple de requête pour une facture d'électricité :
        /// ```json
        /// {
        ///   "BillerCode": "EGY-ELECTRICITY",
        ///   "CustomerReference": "123456789"
        /// }
        /// ```
        /// 
        /// Exemple de requête pour une recharge télécom :
        /// ```json
        /// {
        ///   "BillerCode": "EGY-ORANGE",
        ///   "PhoneNumber": "0101234567"
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Requête de consultation</param>
        /// <returns>Détails de la facture ou validation du numéro de téléphone</returns>
        /// <response code="200">Opération réussie</response>
        /// <response code="400">Requête invalide</response>
        /// <response code="404">Facture ou opérateur non trouvé</response>
        [HttpPost("inquire")]        [ProducesResponseType(typeof(InquiryResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "Admin,Manager,User")]
        public IActionResult Inquire([FromBody] Dictionary<string, object> request)
        {
            _auditLogger.LogAction("Consultation facture/téléphone", $"Payload={System.Text.Json.JsonSerializer.Serialize(request)}");

            if (request == null)
            {
                return BadRequest("Requête invalide");
            }

            // Vérifier les paramètres obligatoires
            if (!request.TryGetValue("BillerCode", out var billerCodeObj))
            {
                return BadRequest("Le champ 'BillerCode' est requis");
            }

            var billerCode = billerCodeObj.ToString();

            // Créer une requête B3G standard
            var b3gRequest = new B3gServiceRequest
            {
                SessionId = Guid.NewGuid().ToString(),
                ServiceId = "inquiry_service",
                UserName = "api_user",
                Password = "api_password",
                Language = "fr",
                ChannelId = "API",
                IsDemo = 1,
                ParamIn = new Dictionary<string, object>
            {
                { "Operation", "INQUIRE" },
                { "BillerCode", billerCode }
            }
            };

            // Ajouter CustomerReference ou PhoneNumber selon le cas
            if (request.TryGetValue("CustomerReference", out var customerRefObj))
            {
                b3gRequest.ParamIn.Add("CustomerReference", customerRefObj);
            }
            else if (request.TryGetValue("PhoneNumber", out var phoneObj))
            {
                b3gRequest.ParamIn.Add("PhoneNumber", phoneObj);
            }
            else
            {
                return BadRequest("Le champ 'CustomerReference' ou 'PhoneNumber' est requis");
            }

            // Si d'autres paramètres sont fournis, les ajouter
            foreach (var param in request)
            {
                if (param.Key != "BillerCode" && param.Key != "CustomerReference" && param.Key != "PhoneNumber"
                    && !b3gRequest.ParamIn.ContainsKey(param.Key))
                {
                    b3gRequest.ParamIn.Add(param.Key, param.Value);
                }
            }

            // Traiter la demande
            var response = _paymentService.Process(b3gRequest);

            // Vérifier s'il y a des erreurs
            if (response != null && response.Count > 0)
            {
                var result = response[0];

                if (result.StatusCode != "000")
                {
                    // Une erreur s'est produite
                    if (result.StatusCode == "200" || result.StatusCode == "202")
                    {
                        // Créancier ou facture non trouvé
                        return NotFound(result.ParamOut);
                    }

                    // Autre erreur
                    return BadRequest(result.ParamOut);
                }

                // Succès, retourner le ParamOut
                return Ok(result.ParamOut);
            }

            // Erreur inattendue
            return StatusCode(500, new { Error = "Erreur serveur interne" });
        }

        /// <summary>
        /// Récupère la liste des créanciers disponibles pour un type de service donné
        /// </summary>
        /// <param name="serviceType">Type de service (BILL_PAYMENT ou TELECOM_RECHARGE)</param>
        /// <returns>Liste des créanciers</returns>
        [HttpGet("billers/{serviceType}")]
        [ProducesResponseType(typeof(List<BillerConfiguration>), 200)]
        public async Task<IActionResult> GetBillersByServiceType(
            [FromRoute] string serviceType,
            [FromServices] BillerConfigService billerConfigService)
        {
            var billers = await billerConfigService.GetBillersByServiceTypeAsync(serviceType);

            // Filtrer les informations à retourner (pour ne pas exposer les détails de configuration)
            var result = billers.Select(b => new
            {
                b.BillerCode,
                b.BillerName,
                b.Description,
                b.Category,
                AvailableAmounts = !string.IsNullOrEmpty(b.AvailableAmounts)
                    ? JsonSerializer.Deserialize<List<decimal>>(b.AvailableAmounts)
                    : null
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Récupère plusieurs factures pour un client donné
        /// </summary>
        /// <param name="request">Requête de consultation multiple</param>
        /// <returns>Liste des factures trouvées</returns>
        [HttpPost("inquire-multiple")]
        [ProducesResponseType(typeof(List<object>), 200)]
        [ProducesResponseType(400)]
        [Authorize(Roles = "Admin,Manager,User")]
        public IActionResult InquireMultiple([FromBody] Dictionary<string, object> request)
        {
            _auditLogger.LogAction("Consultation factures multiples", $"Payload={System.Text.Json.JsonSerializer.Serialize(request)}");

            if (request == null)
            {
                return BadRequest("Requête invalide");
            }

            // Vérifier les paramètres obligatoires
            if (!request.TryGetValue("BillerCode", out var billerCodeObj))
            {
                return BadRequest("Le champ 'BillerCode' est requis");
            }

            if (!request.TryGetValue("CustomerReference", out var customerRefObj))
            {
                return BadRequest("Le champ 'CustomerReference' est requis");
            }

            var billerCode = billerCodeObj.ToString();
            var customerRef = customerRefObj.ToString();

            // Créer une requête B3G standard
            var b3gRequest = new B3gServiceRequest
            {
                SessionId = Guid.NewGuid().ToString(),
                ServiceId = "inquiry_multiple_service",
                UserName = "api_user",
                Password = "api_password",
                Language = "fr",
                ChannelId = "API",
                IsDemo = 1,
                ParamIn = new Dictionary<string, object>
            {
                { "Operation", "INQUIRE_MULTIPLE" },
                { "BillerCode", billerCode },
                { "CustomerReference", customerRef }
            }
            };

            // Si d'autres paramètres sont fournis, les ajouter
            foreach (var param in request)
            {
                if (param.Key != "BillerCode" && param.Key != "CustomerReference"
                    && !b3gRequest.ParamIn.ContainsKey(param.Key))
                {
                    b3gRequest.ParamIn.Add(param.Key, param.Value);
                }
            }

            // Traiter la demande
            var response = _paymentService.Process(b3gRequest);

            // Vérifier s'il y a des erreurs
            if (response != null && response.Count > 0)
            {
                var result = response[0];

                if (result.StatusCode != "000")
                {
                    // Une erreur s'est produite
                    if (result.StatusCode == "200" || result.StatusCode == "202")
                    {
                        // Créancier ou facture non trouvé
                        return NotFound(result.ParamOut);
                    }

                    // Autre erreur
                    return BadRequest(result.ParamOut);
                }

                // Succès, retourner le ParamOut
                return Ok(result.ParamOut);
            }

            // Erreur inattendue
            return StatusCode(500, new { Error = "Erreur serveur interne" });
        }
    }
}
