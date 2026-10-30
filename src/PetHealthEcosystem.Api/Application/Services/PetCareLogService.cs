using Microsoft.Extensions.Logging;
using PetHealthEcosystem.Api.Application.Interfaces;
using PetHealthEcosystem.Api.Domain.Interfaces;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Application.Services
{
    public class PetCareLogService : IPetCareLogService
    {
        private readonly IPetCareLogRepository _repository;
        private readonly ILogger<PetCareLogService> _logger;

        public PetCareLogService(IPetCareLogRepository repository, ILogger<PetCareLogService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<PetCareLog>> GetAllAsync()
        {
            _logger.LogInformation("Consultando todos os logs clínicos NoSQL (MongoDB)");
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<PetCareLog>> GetByPetIdAsync(int petId)
        {
            _logger.LogInformation("Consultando histórico NoSQL de cuidados do Pet {PetId}", petId);
            return await _repository.GetByPetIdAsync(petId);
        }

        public async Task<PetCareLog?> GetByIdAsync(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<PetCareLog> CreateAsync(PetCareLog log)
        {
            if (string.IsNullOrWhiteSpace(log.ActionType))
                log.ActionType = "Cuidado Clínico";

            log.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(log);
            _logger.LogInformation("Log clínico NoSQL registrado com sucesso para Pet {PetId}: {ActionType}", log.PetId, log.ActionType);
            return log;
        }
    }
}
