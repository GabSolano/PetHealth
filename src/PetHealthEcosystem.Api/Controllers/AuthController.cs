using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHealthEcosystem.Api.Application.DTOs.Auth;
using PetHealthEcosystem.Api.Application.Interfaces;

namespace PetHealthEcosystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Realiza autenticação no ecossistema e gera o token JWT.
        /// Usuários de teste disponíveis:
        /// admin / Admin@123 (Role: Admin)
        /// veterinario / Vet@123 (Role: Veterinario)
        /// tutor / Tutor@123 (Role: Tutor)
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var result = _authService.Authenticate(request);
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
    }
}
