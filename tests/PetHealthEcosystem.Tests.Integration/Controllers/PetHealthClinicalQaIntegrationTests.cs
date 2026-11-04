using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PetHealthEcosystem.Api.Application.DTOs.Clinical;
using PetHealthEcosystem.Api.Models;
using PetHealthEcosystem.Tests.Integration.Helpers;
using Xunit;

namespace PetHealthEcosystem.Tests.Integration.Controllers
{
    /// <summary>
    /// Suíte de testes de integração voltada para automação de QA dos fluxos clínicos do PetHealth:
    /// - Check-in de Pós-Operatório (simulação de canal WhatsApp)
    /// - Alertas Críticos de Triagem
    /// - Registro de Vacinas e Prontuário Consolidado
    /// </summary>
    [Collection("Pets API collection")]
    public class PetHealthClinicalQaIntegrationTests
    {
        private readonly HttpClient _client;

        public PetHealthClinicalQaIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostOpCheckIn_SimulandoWhatsAppComRecuperacaoNormal_DeveRetornarOkEStatusEmRecuperacao()
        {
            // Arrange: 1. Cadastra pet operado
            var pet = new Pet
            {
                Name = "Luke",
                Species = "Canino",
                Breed = "Shih Tzu",
                Age = 2,
                TutorName = "Mariana",
                TutorPhone = "+5511988887777",
                NeedsPostOpCare = true,
                SurgeryType = "Castração"
            };

            var createRes = await _client.PostAsJsonAsync("/api/pets", pet);
            var petCriado = await createRes.Content.ReadFromJsonAsync<Pet>();
            Assert.NotNull(petCriado);

            // 2. Simula envio de resposta do tutor via WhatsApp
            var checkIn = new PostOpCheckInRequest
            {
                WaterIntake = "Normal",
                FoodIntake = "Normal",
                PainLevel = 1,
                HasVomiting = false,
                WoundCondition = "Normal",
                Observations = "Luke comeu ração úmida e bebeu água normalmente.",
                SubmittedBy = "Tutor via WhatsApp (IA Conversacional)"
            };

            // Act
            var checkInRes = await _client.PostAsJsonAsync($"/api/pets/{petCriado!.Id}/post-op-checkin", checkIn);

            // Assert
            Assert.Equal(HttpStatusCode.OK, checkInRes.StatusCode);

            // Verifica o estado atualizado do pet
            var getPetRes = await _client.GetAsync($"/api/pets/{petCriado.Id}");
            var petAtualizado = await getPetRes.Content.ReadFromJsonAsync<Pet>();
            Assert.NotNull(petAtualizado);
            Assert.Equal("Em Recuperação", petAtualizado!.RecoveryStatus);
        }

        [Fact]
        public async Task PostOpCheckIn_ComSangramentoEDorSevera_DeveAcionarAlertaCritico()
        {
            // Arrange
            var pet = new Pet
            {
                Name = "Spike",
                Species = "Canino",
                Breed = "Pitbull",
                Age = 4,
                TutorName = "Roberto",
                NeedsPostOpCare = true,
                SurgeryType = "Cirurgia Ortopédica"
            };

            var createRes = await _client.PostAsJsonAsync("/api/pets", pet);
            var petCriado = await createRes.Content.ReadFromJsonAsync<Pet>();
            Assert.NotNull(petCriado);

            var checkInCritico = new PostOpCheckInRequest
            {
                WaterIntake = "Recusa",
                FoodIntake = "Recusa",
                PainLevel = 9,
                HasVomiting = true,
                WoundCondition = "Sangramento",
                Observations = "Spike está chorando de dor e houve sangramento na incisão.",
                SubmittedBy = "Tutor via WhatsApp"
            };

            // Act
            var checkInRes = await _client.PostAsJsonAsync($"/api/pets/{petCriado!.Id}/post-op-checkin", checkInCritico);

            // Assert
            Assert.Equal(HttpStatusCode.OK, checkInRes.StatusCode);

            var getPetRes = await _client.GetAsync($"/api/pets/{petCriado.Id}");
            var petAtualizado = await getPetRes.Content.ReadFromJsonAsync<Pet>();
            Assert.NotNull(petAtualizado);
            Assert.Equal("Alerta Crítico", petAtualizado!.RecoveryStatus);
        }

        [Fact]
        public async Task RegisterVaccine_ComPerfilVeterinario_DeveSalvarCarteiraVacinal()
        {
            // Arrange
            var pet = new Pet
            {
                Name = "Bela",
                Species = "Felino",
                Breed = "Siamês",
                Age = 1,
                TutorName = "Fernanda"
            };

            var createRes = await _client.PostAsJsonAsync("/api/pets", pet);
            var petCriado = await createRes.Content.ReadFromJsonAsync<Pet>();
            Assert.NotNull(petCriado);

            var vacina = new VaccineRegistrationRequest
            {
                VaccineName = "V4 Felina",
                Dose = "1ª Dose",
                Veterinarian = "Dr. Silva",
                Notes = "Aplicação sem reações imediatas."
            };

            // Act: Envia requisição com perfil de Veterinário
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/pets/{petCriado!.Id}/vaccines")
            {
                Content = JsonContent.Create(vacina)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", QaAuthTokenHelper.GetVeterinarioToken());

            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Consulta o prontuário para validar o registro
            var medicalRecordRes = await _client.GetAsync($"/api/pets/{petCriado.Id}/medical-record");
            Assert.Equal(HttpStatusCode.OK, medicalRecordRes.StatusCode);

            var record = await medicalRecordRes.Content.ReadFromJsonAsync<PetMedicalRecordDto>();
            Assert.NotNull(record);
            Assert.Contains(record!.Vaccines, v => v.VaccineName == "V4 Felina");
        }
    }
}
