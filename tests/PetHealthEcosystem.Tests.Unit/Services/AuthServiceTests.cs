using Microsoft.Extensions.Configuration;
using PetHealthEcosystem.Api.Application.DTOs.Auth;
using PetHealthEcosystem.Api.Application.Services;
using Xunit;

namespace PetHealthEcosystem.Tests.Unit.Services
{
    public class AuthServiceTests
    {
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "TestSecretKeyLongEnoughForHmacSha256Security123456!" },
                { "Jwt:Issuer", "PetHealthEcosystemTest" },
                { "Jwt:Audience", "PetHealthEcosystemTestUsers" },
                { "Jwt:ExpirationHours", "2" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _sut = new AuthService(configuration);
        }

        [Fact]
        public void Authenticate_ComCredenciaisValidasAdmin_DeveRetornarTokenERoleAdmin()
        {
            // Arrange
            var request = new LoginRequest { Username = "admin", Password = "Admin@123" };

            // Act
            var response = _sut.Authenticate(request);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Token);
            Assert.Equal("admin", response.Username);
            Assert.Equal("Admin", response.Role);
            Assert.NotNull(response.Expiration);
        }

        [Fact]
        public void Authenticate_ComCredenciaisValidasVeterinario_DeveRetornarRoleVeterinario()
        {
            // Arrange
            var request = new LoginRequest { Username = "veterinario", Password = "Vet@123" };

            // Act
            var response = _sut.Authenticate(request);

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Veterinario", response.Role);
        }

        [Fact]
        public void Authenticate_ComSenhaInvalida_DeveRetornarFalha()
        {
            // Arrange
            var request = new LoginRequest { Username = "admin", Password = "WrongPassword" };

            // Act
            var response = _sut.Authenticate(request);

            // Assert
            Assert.False(response.Success);
            Assert.Null(response.Token);
            Assert.Equal("Credenciais inválidas.", response.Message);
        }

        [Fact]
        public void Authenticate_ComUsuarioInexistente_DeveRetornarFalha()
        {
            // Arrange
            var request = new LoginRequest { Username = "usuario_inexistente", Password = "any" };

            // Act
            var response = _sut.Authenticate(request);

            // Assert
            Assert.False(response.Success);
            Assert.Null(response.Token);
            Assert.Equal("Credenciais inválidas.", response.Message);
        }

        [Theory]
        [InlineData("", "Admin@123")]
        [InlineData("admin", "")]
        [InlineData("", "")]
        public void Authenticate_ComCredenciaisVazias_DeveRetornarFalha(string username, string password)
        {
            // Arrange
            var request = new LoginRequest { Username = username, Password = password };

            // Act
            var response = _sut.Authenticate(request);

            // Assert
            Assert.False(response.Success);
            Assert.Null(response.Token);
        }
    }
}
