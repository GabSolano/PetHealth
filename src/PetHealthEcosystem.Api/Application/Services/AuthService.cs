using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PetHealthEcosystem.Api.Application.DTOs.Auth;
using PetHealthEcosystem.Api.Application.Interfaces;

namespace PetHealthEcosystem.Api.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        // Usuários padrão do ecossistema de saúde animal para autenticação
        private static readonly Dictionary<string, (string Password, string Role)> Users = new()
        {
            { "admin", ("Admin@123", "Admin") },
            { "veterinario", ("Vet@123", "Veterinario") },
            { "tutor", ("Tutor@123", "Tutor") }
        };

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthResponse Authenticate(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new AuthResponse { Success = false, Message = "Usuário e senha são obrigatórios." };
            }

            if (!Users.TryGetValue(request.Username.ToLower(), out var user) || user.Password != request.Password)
            {
                return new AuthResponse { Success = false, Message = "Credenciais inválidas." };
            }

            var secretKey = _configuration["Jwt:SecretKey"] ?? "PetHealthEcosystemSuperSecretSecretKey2026Sprint4!";
            var issuer = _configuration["Jwt:Issuer"] ?? "PetHealthEcosystem";
            var audience = _configuration["Jwt:Audience"] ?? "PetHealthEcosystemUsers";
            var expirationHours = int.TryParse(_configuration["Jwt:ExpirationHours"], out var hours) ? hours : 8;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);
            var expires = DateTime.UtcNow.AddHours(expirationHours);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, request.Username),
                    new Claim(ClaimTypes.Name, request.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = expires,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponse
            {
                Success = true,
                Token = tokenHandler.WriteToken(token),
                Username = request.Username,
                Role = user.Role,
                Expiration = expires,
                Message = "Autenticação realizada com sucesso."
            };
        }
    }
}
