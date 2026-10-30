using System.Net;
using System.Net.Http.Json;
using PetHealthEcosystem.Api.Application.DTOs.Hateoas;
using PetHealthEcosystem.Api.Models;
using Xunit;

namespace PetHealthEcosystem.Tests.Integration.Controllers
{
    [Collection("Pets API collection")]
    public class PetsPaginationHateoasIntegrationTests
    {
        private readonly HttpClient _client;

        public PetsPaginationHateoasIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ComPaginacaoEFiltros_DeveRetornarPagedResultComHateoas()
        {
            // Arrange: Cadastra dois pets
            await _client.PostAsJsonAsync("/api/pets", new Pet { Name = "Hulk", Breed = "Pitbull", Age = 3, TutorName = "Joao" });
            await _client.PostAsJsonAsync("/api/pets", new Pet { Name = "Luna", Breed = "Poodle", Age = 2, TutorName = "Maria" });

            // Act
            var response = await _client.GetAsync("/api/pets?pageNumber=1&pageSize=5&sortBy=name&sortOrder=asc");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<Pet>>();
            Assert.NotNull(result);
            Assert.True(result!.Items.Any());
            Assert.Contains(result.Links, l => l.Rel == "self");
            Assert.Contains(result.Links, l => l.Rel == "first");
        }

        [Fact]
        public async Task GetById_DeveRetornarPetComLinksHateoas()
        {
            // Arrange
            var createRes = await _client.PostAsJsonAsync("/api/pets", new Pet { Name = "Max", Breed = "Beagle", Age = 4, TutorName = "Ana" });
            var pet = await createRes.Content.ReadFromJsonAsync<Pet>();

            // Act
            var response = await _client.GetAsync($"/api/pets/{pet!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var petComLinks = await response.Content.ReadFromJsonAsync<Pet>();
            Assert.NotNull(petComLinks);
            Assert.NotNull(petComLinks!.Links);
            Assert.Contains(petComLinks.Links, l => l.Rel == "self");
            Assert.Contains(petComLinks.Links, l => l.Rel == "update");
            Assert.Contains(petComLinks.Links, l => l.Rel == "delete");
            Assert.Contains(petComLinks.Links, l => l.Rel == "care-logs");
        }
    }
}
