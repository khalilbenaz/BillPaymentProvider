using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BillPaymentProvider.Services;
using BillPaymentProvider.Core.Models;
using BillPaymentProvider.Utils;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BillPaymentProvider.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly UserService _userService;
        private readonly BruteForceProtection _bruteForceProtection;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            JwtService jwtService, 
            UserService userService, 
            BruteForceProtection bruteForceProtection,
            ILogger<AuthController> logger)
        {
            _jwtService = jwtService;
            _userService = userService;
            _bruteForceProtection = bruteForceProtection;
            _logger = logger;
        }

        /// <summary>
        /// Authentification utilisateur
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 429)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Tentative de connexion avec des données invalides depuis {IP}", 
                    HttpContext.Connection.RemoteIpAddress);
                return BadRequest(new ErrorResponse("Nom d'utilisateur et mot de passe requis"));
            }

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Protection brute force
            if (_bruteForceProtection.IsLockedOut(request.Username))
            {
                var lockoutTime = _bruteForceProtection.GetLockoutTimeRemaining(request.Username);
                _logger.LogWarning("Tentative de connexion sur compte bloqué {Username} depuis {IP}", 
                    request.Username, clientIp);
                
                return Unauthorized(new ErrorResponse(
                    $"Compte temporairement bloqué. Réessayez dans {lockoutTime?.Minutes} minutes.",
                    "ACCOUNT_LOCKED"));
            }

            var user = await _userService.GetUserByUsernameAsync(request.Username);
            if (user == null || !_userService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _bruteForceProtection.RegisterFailedAttempt(request.Username, clientIp);
                var remainingAttempts = _bruteForceProtection.GetRemainingAttempts(request.Username);
                
                _logger.LogWarning("Échec de connexion pour {Username} depuis {IP}. Tentatives restantes: {Remaining}", 
                    request.Username, clientIp, remainingAttempts);

                return Unauthorized(new ErrorResponse(
                    remainingAttempts > 0 
                        ? $"Identifiants incorrects. {remainingAttempts} tentatives restantes."
                        : "Identifiants incorrects.",
                    "INVALID_CREDENTIALS"));
            }

            _bruteForceProtection.ResetAttempts(request.Username);
            
            var sessionId = Guid.NewGuid().ToString();
            var token = _jwtService.GenerateToken(user.Username, user.Role ?? "User", sessionId);
            
            _logger.LogInformation("Connexion réussie pour {Username} depuis {IP}", 
                user.Username, clientIp);

            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresIn = TimeSpan.FromMinutes(30).TotalSeconds,
                User = new UserInfo
                {
                    Username = user.Username,
                    Role = user.Role ?? "User"
                }
            });
        }

        /// <summary>
        /// Validation du token (pour vérifier si il est encore valide)
        /// </summary>
        [HttpPost("validate")]
        [Authorize]
        [ProducesResponseType(typeof(ValidateTokenResponse), 200)]
        [ProducesResponseType(401)]
        public IActionResult ValidateToken()
        {
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            _logger.LogDebug("Validation de token pour {Username}", username);

            return Ok(new ValidateTokenResponse
            {
                Valid = true,
                Username = username,
                Role = role
            });
        }

        /// <summary>
        /// Déconnexion (optionnel - pour invalider côté client)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        public IActionResult Logout()
        {
            var username = User.Identity?.Name;
            _logger.LogInformation("Déconnexion de {Username}", username);
            
            return Ok(new { message = "Déconnexion réussie" });
        }

        /// <summary>
        /// Obtient les informations du profil utilisateur
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserInfo), 200)]
        public IActionResult GetProfile()
        {
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new UserInfo
            {
                Username = username,
                Role = role ?? "User"
            });
        }
    }

    /// <summary>
    /// Modèle de requête de connexion
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Le nom d'utilisateur doit contenir entre 3 et 50 caractères")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public required string Password { get; set; }
    }

    /// <summary>
    /// Modèle de réponse de connexion
    /// </summary>
    public class LoginResponse
    {
        public required string Token { get; set; }
        public double ExpiresIn { get; set; }
        public required UserInfo User { get; set; }
    }

    /// <summary>
    /// Modèle d'informations utilisateur
    /// </summary>
    public class UserInfo
    {
        public string? Username { get; set; }
        public required string Role { get; set; }
    }

    /// <summary>
    /// Modèle de réponse de validation de token
    /// </summary>
    public class ValidateTokenResponse
    {
        public bool Valid { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
    }

    /// <summary>
    /// Modèle d'erreur standard
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string? Code { get; set; }

        public ErrorResponse(string message, string? code = null)
        {
            Message = message;
            Code = code;
        }
    }
}
