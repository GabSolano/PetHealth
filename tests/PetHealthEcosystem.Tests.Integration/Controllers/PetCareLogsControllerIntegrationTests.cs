using System.Net;
using System.Net.Http.Json;
using PetHealthEcosystem.Api.Models;
using Xunit;

namespace PetHealthEcosystem.Tests.Integration.Controllers
{
    [Collection("Pets API collection")]
    public class PetCareLogsControllerIntegrationTests
    {
        private readonly HttpClient _client;

        public PetCareLogsControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostCareLog_DeveCriarRegistroNoSqlERetornarCreated()
        {
            // Arrange
            var log = new PetCareLog
            {
                PetId = 1,
                PetName = "Thor",
                ActionType = "Medicação Pós-Op",
                Notes = "Administrado antibiótico 50mg.",
                RegisteredBy = "Dr. Silva"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/petcarelogs", log);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<PetCareLog>();
            Assert.NotNull(created);
            Assert.False(string.IsNullOrEmpty(created!.Id));
            Assert.Equal("Thor", created.PetName);
        }

        [Fact]
        public async Task GetAllCareLogs_DeveRetornarOk()
        {
            // Act
            var response = await _client.GetAsync("/api/petcarelogs");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = await response.Content.ReadFromJsonAsync<List<PetCareLog>>();
            Assert.NotNull(list);
        }

        [Fact]
        public async Task GetCareLogsByPetId_DeveRetornarOk()
        {
            // Act
            var response = await _client.GetAsync("/api/petcarelogs/pet/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
