using System.Net;
using System.Net.Http.Json;
using PetHealthEcosystem.Api.Application.DTOs.Auth;
using Xunit;

namespace PetHealthEcosystem.Tests.Integration.Controllers
{
    [Collection("Pets API collection")]
    public class AuthControllerIntegrationTests
    {
        private readonly HttpClient _client;

        public AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_ComCredenciaisValidas_DeveRetornarOkETokenJwt()
        {
            // Arrange
            var request = new LoginRequest { Username = "admin", Password = "Admin@123" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            Assert.NotNull(result);
            Assert.True(result!.Success);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.Equal("Admin", result.Role);
        }

        [Fact]
        public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
        {
            // Arrange
            var request = new LoginRequest { Username = "admin", Password = "SenhaErrada" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
