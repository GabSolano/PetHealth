using System.ComponentModel.DataAnnotations;

namespace PetHealthEcosystem.Api.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "O usuário é obrigatório.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public DateTime? Expiration { get; set; }
        public string? Message { get; set; }
    }
}
