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

namespace BillPaymentProvider.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;

        public AuthController(IConfiguration configuration, UserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return Unauthorized();

            // Protection brute force
            if (BruteForceProtection.IsLockedOut(request.Username))
                return Unauthorized("Compte temporairement bloqué suite à trop de tentatives.");

            var user = await _userService.GetUserByUsernameAsync(request.Username);
            if (user == null || !_userService.VerifyPassword(request.Password, user.PasswordHash))
            {
                BruteForceProtection.RegisterFailedAttempt(request.Username);
                return Unauthorized();
            }

            BruteForceProtection.ResetAttempts(request.Username);
            var token = GenerateJwtToken(user.Username, user.Role ?? "User");
            return Ok(new { token });
        }

        private string GenerateJwtToken(string username, string role)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? string.Empty;
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtLifetimeMinutes = int.TryParse(_configuration["Jwt:LifetimeMinutes"], out var l) ? l : 30;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtLifetimeMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
