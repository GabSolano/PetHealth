using Microsoft.Extensions.Logging;
using Moq;
using PetHealthEcosystem.Api.Application.Services;
using PetHealthEcosystem.Api.Domain.Interfaces;
using PetHealthEcosystem.Api.Models;
using Xunit;

namespace PetHealthEcosystem.Tests.Unit.Services
{
    /// <summary>
    /// Testes unitários da camada de Aplicação (PetService), com o repositório
    /// (camada de Domínio/Infraestrutura) mockado via Moq.
    /// Convenção de nomes: MetodoTestado_Cenario_ResultadoEsperado.
    /// </summary>
    public class PetServiceTests
    {
        private readonly Mock<IPetRepository> _repositoryMock;
        private readonly PetService _sut; // system under test

        public PetServiceTests()
        {
            _repositoryMock = new Mock<IPetRepository>();
            var loggerMock = new Mock<ILogger<PetService>>();
            _sut = new PetService(_repositoryMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_QuandoExistemPets_DeveRetornarTodosOsPets()
        {
            // Arrange
            var petsEsperados = new List<Pet>
            {
                new Pet { Id = 1, Name = "Rex", Breed = "Labrador", Age = 3, TutorName = "Ana" },
                new Pet { Id = 2, Name = "Mia", Breed = "Siamês", Age = 2, TutorName = "Bruno" }
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(petsEsperados);

            // Act
            var resultado = await _sut.GetAllAsync();

            // Assert
            Assert.Equal(2, resultado.Count());
            _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_QuandoPetNaoExiste_DeveRetornarNulo()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Pet?)null);

            // Act
            var resultado = await _sut.GetByIdAsync(99);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task CreateAsync_ComNomeVazio_DeveRetornarFalhaDeValidacaoSemPersistir()
        {
            // Arrange
            var petInvalido = new Pet { Name = "", Breed = "Vira-lata", Age = 1, TutorName = "Carla" };

            // Act
            var (sucesso, erro, petCriado) = await _sut.CreateAsync(petInvalido);

            // Assert
            Assert.False(sucesso);
            Assert.Equal("O nome do pet é obrigatório.", erro);
            Assert.Null(petCriado);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Pet>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ComIdadeNegativa_DeveRetornarFalhaDeValidacao()
        {
            // Arrange
            var petInvalido = new Pet { Name = "Thor", Breed = "Pastor Alemão", Age = -1, TutorName = "Diego" };

            // Act
            var (sucesso, erro, _) = await _sut.CreateAsync(petInvalido);

            // Assert
            Assert.False(sucesso);
            Assert.Equal("A idade do pet não pode ser negativa.", erro);
        }

        [Fact]
        public async Task CreateAsync_SemNomeDoTutor_DeveRetornarFalhaDeValidacao()
        {
            // Arrange
            var petInvalido = new Pet { Name = "Amora", Breed = "SRD", Age = 2, TutorName = "" };

            // Act
            var (sucesso, erro, _) = await _sut.CreateAsync(petInvalido);

            // Assert
            Assert.False(sucesso);
            Assert.Equal("O nome do tutor é obrigatório.", erro);
        }

        [Fact]
        public async Task CreateAsync_ComDadosValidos_DevePersistirERetornarSucesso()
        {
            // Arrange
            var petValido = new Pet { Name = "Nina", Breed = "Poodle", Age = 4, TutorName = "Elisa" };
            _repositoryMock.Setup(r => r.AddAsync(petValido)).Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var (sucesso, erro, petCriado) = await _sut.CreateAsync(petValido);

            // Assert
            Assert.True(sucesso);
            Assert.Null(erro);
            Assert.Equal(petValido, petCriado);
            _repositoryMock.Verify(r => r.AddAsync(petValido), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ComIdDivergenteDoCorpo_DeveRetornarFalha()
        {
            // Arrange
            var pet = new Pet { Id = 5, Name = "Bob", Breed = "Vira-lata", Age = 2, TutorName = "Fábio" };

            // Act
            var (sucesso, erro) = await _sut.UpdateAsync(1, pet);

            // Assert
            Assert.False(sucesso);
            Assert.Equal("ID incompatível.", erro);
        }

        [Fact]
        public async Task UpdateAsync_QuandoPetNaoExiste_DeveRetornarFalha()
        {
            // Arrange
            var pet = new Pet { Id = 10, Name = "Bob", Breed = "Vira-lata", Age = 2, TutorName = "Fábio" };
            _repositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Pet?)null);

            // Act
            var (sucesso, erro) = await _sut.UpdateAsync(10, pet);

            // Assert
            Assert.False(sucesso);
            Assert.Equal("Pet não encontrado.", erro);
        }

        [Fact]
        public async Task UpdateAsync_ComDadosValidos_DeveAtualizarCamposERetornarSucesso()
        {
            // Arrange
            var existente = new Pet { Id = 7, Name = "Old", Breed = "SRD", Age = 1, TutorName = "Gil" };
            var atualizado = new Pet { Id = 7, Name = "New", Breed = "SRD", Age = 2, TutorName = "Gil", NeedsPostOpCare = true };
            _repositoryMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(existente);

            // Act
            var (sucesso, erro) = await _sut.UpdateAsync(7, atualizado);

            // Assert
            Assert.True(sucesso);
            Assert.Null(erro);
            Assert.Equal("New", existente.Name);
            Assert.True(existente.NeedsPostOpCare);
            _repositoryMock.Verify(r => r.UpdateAsync(existente), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_QuandoPetExiste_DeveRemoverERetornarTrue()
        {
            // Arrange
            var pet = new Pet { Id = 3, Name = "Luna", Breed = "SRD", Age = 5, TutorName = "Helena" };
            _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(pet);

            // Act
            var resultado = await _sut.DeleteAsync(3);

            // Assert
            Assert.True(resultado);
            _repositoryMock.Verify(r => r.DeleteAsync(pet), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_QuandoPetNaoExiste_DeveRetornarFalseSemRemover()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Pet?)null);

            // Act
            var resultado = await _sut.DeleteAsync(999);

            // Assert
            Assert.False(resultado);
            _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Pet>()), Times.Never);
        }

        [Fact]
        public async Task GetPagedAsync_ComParametrosDePaginacao_DeveRetornarResultadoPaginado()
        {
            // Arrange
            var pets = new List<Pet>
            {
                new Pet { Id = 1, Name = "Rex", Breed = "Golden", Age = 3, TutorName = "Ana" }
            };
            var query = new Api.Application.DTOs.Pets.PetQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                Breed = "Golden"
            };

            _repositoryMock.Setup(r => r.GetPagedAsync(query)).ReturnsAsync((pets, 1));

            // Act
            var resultado = await _sut.GetPagedAsync(query);

            // Assert
            Assert.NotNull(resultado);
            Assert.Single(resultado.Items);
            Assert.Equal(1, resultado.TotalItems);
            Assert.Equal(1, resultado.PageNumber);
            _repositoryMock.Verify(r => r.GetPagedAsync(query), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ComCareLogRepository_DeveGravarLogNoSql()
        {
            // Arrange
            var careLogRepoMock = new Mock<IPetCareLogRepository>();
            var loggerMock = new Mock<ILogger<PetService>>();
            var serviceComNoSql = new PetService(_repositoryMock.Object, loggerMock.Object, careLogRepoMock.Object);

            var pet = new Pet { Name = "Thor", Breed = "Husky", Age = 2, TutorName = "Marcos" };
            _repositoryMock.Setup(r => r.AddAsync(pet)).Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            careLogRepoMock.Setup(c => c.AddAsync(It.IsAny<PetCareLog>())).Returns(Task.CompletedTask);

            // Act
            var (sucesso, _, _) = await serviceComNoSql.CreateAsync(pet, "Dr. Veterinario");

            // Assert
            Assert.True(sucesso);
            careLogRepoMock.Verify(c => c.AddAsync(It.Is<PetCareLog>(l => l.PetName == "Thor" && l.ActionType == "Cadastro")), Times.Once);
        }

        [Fact]
        public async Task RegisterPostOpCheckInAsync_ComSintomasNormais_DeveRegistrarEmRecuperacao()
        {
            // Arrange
            var careLogRepoMock = new Mock<IPetCareLogRepository>();
            var loggerMock = new Mock<ILogger<PetService>>();
            var service = new PetService(_repositoryMock.Object, loggerMock.Object, careLogRepoMock.Object);

            var pet = new Builders.PetTestDataBuilder().WithId(10).InPostOpRecovery().Build();
            _repositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(pet);

            var checkIn = Builders.PostOpCheckInBuilder.CreateHealthy().Build();

            // Act
            var (success, error, log) = await service.RegisterPostOpCheckInAsync(10, checkIn);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.NotNull(log);
            Assert.False(log!.IsCriticalAlert);
            Assert.Equal("Em Recuperação", pet.RecoveryStatus);
            careLogRepoMock.Verify(c => c.AddAsync(It.Is<PetCareLog>(l => l.PetId == 10 && !l.IsCriticalAlert)), Times.Once);
        }

        [Fact]
        public async Task RegisterPostOpCheckInAsync_ComDorSeveraESangramento_DeveAcionarAlertaCritico()
        {
            // Arrange
            var careLogRepoMock = new Mock<IPetCareLogRepository>();
            var loggerMock = new Mock<ILogger<PetService>>();
            var service = new PetService(_repositoryMock.Object, loggerMock.Object, careLogRepoMock.Object);

            var pet = new Builders.PetTestDataBuilder().WithId(12).InPostOpRecovery().Build();
            _repositoryMock.Setup(r => r.GetByIdAsync(12)).ReturnsAsync(pet);

            var checkInCritico = Builders.PostOpCheckInBuilder.CreateCritical().Build();

            // Act
            var (success, _, log) = await service.RegisterPostOpCheckInAsync(12, checkInCritico);

            // Assert
            Assert.True(success);
            Assert.NotNull(log);
            Assert.True(log!.IsCriticalAlert);
            Assert.Equal("Alerta Crítico", pet.RecoveryStatus);
            careLogRepoMock.Verify(c => c.AddAsync(It.Is<PetCareLog>(l => l.PetId == 12 && l.IsCriticalAlert)), Times.Once);
        }

        [Fact]
        public async Task RegisterVaccineAsync_ComDadosValidos_DeveCalcularProximoReforcoEGravar()
        {
            // Arrange
            var careLogRepoMock = new Mock<IPetCareLogRepository>();
            var loggerMock = new Mock<ILogger<PetService>>();
            var service = new PetService(_repositoryMock.Object, loggerMock.Object, careLogRepoMock.Object);

            var pet = new Builders.PetTestDataBuilder().WithId(15).Build();
            _repositoryMock.Setup(r => r.GetByIdAsync(15)).ReturnsAsync(pet);

            var vaccineRequest = new Api.Application.DTOs.Clinical.VaccineRegistrationRequest
            {
                VaccineName = "V10 PoliValente",
                Dose = "Reforço Anual",
                ApplicationDate = new DateTime(2026, 11, 4),
                Veterinarian = "Dra. Carolina"
            };

            // Act
            var (success, _, log) = await service.RegisterVaccineAsync(15, vaccineRequest);

            // Assert
            Assert.True(success);
            Assert.NotNull(log);
            Assert.Equal("V10 PoliValente", log!.VaccineName);
            Assert.NotNull(log.NextDueDate);
            Assert.Equal(new DateTime(2027, 11, 4), log.NextDueDate!.Value.Date);
            careLogRepoMock.Verify(c => c.AddAsync(It.Is<PetCareLog>(l => l.VaccineName == "V10 PoliValente")), Times.Once);
        }

        [Fact]
        public async Task GetMedicalRecordAsync_QuandoPetExiste_DeveConsolidarProntuario()
        {
            // Arrange
            var careLogRepoMock = new Mock<IPetCareLogRepository>();
            var loggerMock = new Mock<ILogger<PetService>>();
            var service = new PetService(_repositoryMock.Object, loggerMock.Object, careLogRepoMock.Object);

            var pet = new Builders.PetTestDataBuilder().WithId(20).WithName("Mel").Build();
            _repositoryMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(pet);

            var logs = new List<PetCareLog>
            {
                new PetCareLog { PetId = 20, LogType = "Vacinacao", VaccineName = "Antirrábica" },
                new PetCareLog { PetId = 20, LogType = "CheckInPosOp", IsCriticalAlert = false }
            };
            careLogRepoMock.Setup(c => c.GetByPetIdAsync(20)).ReturnsAsync(logs);

            // Act
            var record = await service.GetMedicalRecordAsync(20);

            // Assert
            Assert.NotNull(record);
            Assert.Equal("Mel", record!.Pet.Name);
            Assert.Single(record.Vaccines);
            Assert.Single(record.CheckIns);
            Assert.Equal(2, record.FullHistory.Count());
        }
    }
}
