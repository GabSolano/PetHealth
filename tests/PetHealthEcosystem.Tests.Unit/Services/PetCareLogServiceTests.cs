using Microsoft.Extensions.Logging;
using Moq;
using PetHealthEcosystem.Api.Application.Services;
using PetHealthEcosystem.Api.Domain.Interfaces;
using PetHealthEcosystem.Api.Models;
using Xunit;

namespace PetHealthEcosystem.Tests.Unit.Services
{
    public class PetCareLogServiceTests
    {
        private readonly Mock<IPetCareLogRepository> _repoMock;
        private readonly PetCareLogService _sut;

        public PetCareLogServiceTests()
        {
            _repoMock = new Mock<IPetCareLogRepository>();
            var loggerMock = new Mock<ILogger<PetCareLogService>>();
            _sut = new PetCareLogService(_repoMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ComDadosValidos_DeveGravarLogNoSqlERetornar()
        {
            // Arrange
            var log = new PetCareLog
            {
                PetId = 1,
                PetName = "Thor",
                ActionType = "Cirurgia",
                Notes = "Cirurgia ortopédica realizada com sucesso.",
                RegisteredBy = "Dr. Marcelo"
            };

            _repoMock.Setup(r => r.AddAsync(It.IsAny<PetCareLog>())).Returns(Task.CompletedTask);

            // Act
            var resultado = await _sut.CreateAsync(log);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("Cirurgia", resultado.ActionType);
            _repoMock.Verify(r => r.AddAsync(log), Times.Once);
        }

        [Fact]
        public async Task GetByPetIdAsync_DeveRetornarLogsFiltradosPorPet()
        {
            // Arrange
            var logs = new List<PetCareLog>
            {
                new PetCareLog { Id = "1", PetId = 5, ActionType = "Vacina" },
                new PetCareLog { Id = "2", PetId = 5, ActionType = "Consulta" }
            };

            _repoMock.Setup(r => r.GetByPetIdAsync(5)).ReturnsAsync(logs);

            // Act
            var resultado = await _sut.GetByPetIdAsync(5);

            // Assert
            Assert.Equal(2, resultado.Count());
            _repoMock.Verify(r => r.GetByPetIdAsync(5), Times.Once);
        }
    }
}
