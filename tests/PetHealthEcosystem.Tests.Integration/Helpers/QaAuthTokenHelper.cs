using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PetHealthEcosystem.Tests.Integration.Helpers
{
    /// <summary>
    /// Utilitário de QA para geração ágil de tokens JWT válidos em pipelines
    /// de testes automatizados e suítes de regressão (Postman, xUnit, Playwright, etc.).
    /// </summary>
    public static class QaAuthTokenHelper
    {
        private const string SecretKey = "PetHealthEcosystemSuperSecretSecretKey2026Sprint4!";
        private const string Issuer = "PetHealthEcosystem";
        private const string Audience = "PetHealthEcosystemUsers";

        public static string GenerateToken(string username = "qa_tester", string role = "Admin")
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, username),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = Issuer,
                Audience = Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string GetAdminToken() => GenerateToken("admin_qa", "Admin");
        public static string GetVeterinarioToken() => GenerateToken("vet_qa", "Veterinario");
        public static string GetTutorToken() => GenerateToken("tutor_qa", "Tutor");
    }
}
