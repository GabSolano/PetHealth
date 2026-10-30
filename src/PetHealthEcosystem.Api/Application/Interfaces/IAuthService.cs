using PetHealthEcosystem.Api.Application.DTOs.Auth;

namespace PetHealthEcosystem.Api.Application.Interfaces
{
    public interface IAuthService
    {
        AuthResponse Authenticate(LoginRequest request);
    }
}
