using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BillPaymentProvider.Services
{
    /// <summary>
    /// Service centralisé pour la gestion des tokens JWT
    /// </summary>
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Génère un token JWT sécurisé
        /// </summary>
        public string GenerateToken(string username, string role, string? sessionId = null)
        {
            var jwtKey = GetJwtKey();
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "BillPaymentProvider";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "BillPaymentProvider";
            var jwtLifetimeMinutes = int.TryParse(_configuration["Jwt:LifetimeMinutes"], out var l) ? l : 30;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(ClaimTypes.Name, username),
                new(ClaimTypes.Role, role)
            };

            if (!string.IsNullOrEmpty(sessionId))
            {
                claims.Add(new Claim("session_id", sessionId));
            }

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtLifetimeMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            _logger.LogInformation("Token JWT généré pour l'utilisateur {Username} avec le rôle {Role}", 
                username, role);

            return tokenString;
        }

        /// <summary>
        /// Valide un token JWT
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var jwtKey = GetJwtKey();
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? "BillPaymentProvider";
                var jwtAudience = _configuration["Jwt:Audience"] ?? "BillPaymentProvider";

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Échec de validation du token JWT: {Error}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Génère une clé JWT sécurisée si elle n'existe pas
        /// </summary>
        private string GetJwtKey()
        {
            var jwtKey = _configuration["Jwt:Key"];
            
            if (string.IsNullOrEmpty(jwtKey) || jwtKey == "votre_cle_secrete_super_longue_a_personnaliser")
            {
                _logger.LogWarning("Clé JWT par défaut détectée. Génération d'une clé sécurisée...");
                jwtKey = GenerateSecureKey();
            }

            if (jwtKey.Length < 32)
            {
                throw new InvalidOperationException("La clé JWT doit faire au moins 32 caractères pour garantir la sécurité");
            }

            return jwtKey;
        }

        /// <summary>
        /// Génère une clé cryptographiquement sécurisée
        /// </summary>
        private static string GenerateSecureKey()
        {
            using var rng = RandomNumberGenerator.Create();
            var keyBytes = new byte[64]; // 512 bits
            rng.GetBytes(keyBytes);
            return Convert.ToBase64String(keyBytes);
        }

        /// <summary>
        /// Extrait le nom d'utilisateur du token
        /// </summary>
        public string? GetUsernameFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.Identity?.Name;
        }

        /// <summary>
        /// Extrait le rôle du token
        /// </summary>
        public string? GetRoleFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
