using Microsoft.Extensions.Logging;
using PetHealthEcosystem.Api.Application.DTOs.Hateoas;
using PetHealthEcosystem.Api.Application.DTOs.Pets;
using PetHealthEcosystem.Api.Application.Interfaces;
using PetHealthEcosystem.Api.Domain.Interfaces;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Application.Services
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _repository;
        private readonly ILogger<PetService> _logger;
        private readonly IPetCareLogRepository? _careLogRepository;

        public PetService(
            IPetRepository repository, 
            ILogger<PetService> logger, 
            IPetCareLogRepository? careLogRepository = null)
        {
            _repository = repository;
            _logger = logger;
            _careLogRepository = careLogRepository;
        }

        public async Task<IEnumerable<Pet>> GetAllAsync()
        {
            _logger.LogInformation("Buscando todos os pets cadastrados");
            return await _repository.GetAllAsync();
        }

        public async Task<PagedResult<Pet>> GetPagedAsync(PetQueryParameters parameters)
        {
            _logger.LogInformation("Buscando pets com paginação e filtros (Página: {Page}, Tamanho: {Size})", parameters.PageNumber, parameters.PageSize);
            var (items, totalCount) = await _repository.GetPagedAsync(parameters);

            return new PagedResult<Pet>
            {
                Items = items,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalItems = totalCount
            };
        }

        public async Task<Pet?> GetByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<Pet>> GetByBreedAsync(string breed) =>
            await _repository.GetByBreedAsync(breed);

        public async Task<IEnumerable<Pet>> GetByPostOpCareAsync(bool needsCare) =>
            await _repository.GetByPostOpCareAsync(needsCare);

        public async Task<IEnumerable<Pet>> GetByTutorAsync(string tutorName) =>
            await _repository.GetByTutorAsync(tutorName);

        public async Task<(bool Success, string? Error, Pet? Pet)> CreateAsync(Pet pet, string? author = null)
        {
            var validationError = Validate(pet);
            if (validationError != null)
            {
                _logger.LogWarning("Falha de validação ao criar pet: {Erro}", validationError);
                return (false, validationError, null);
            }

            await _repository.AddAsync(pet);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Pet {PetId} criado com sucesso", pet.Id);

            if (_careLogRepository != null)
            {
                try
                {
                    await _careLogRepository.AddAsync(new PetCareLog
                    {
                        PetId = pet.Id,
                        PetName = pet.Name,
                        ActionType = "Cadastro",
                        Notes = $"Pet cadastrado no sistema. Raça: {pet.Breed}, Idade: {pet.Age}, Cuidados pós-op: {pet.NeedsPostOpCare}",
                        RegisteredBy = author ?? pet.TutorName,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao gravar log clínico NoSQL para Pet {PetId}", pet.Id);
                }
            }

            return (true, null, pet);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, Pet pet, string? author = null)
        {
            if (id != pet.Id)
                return (false, "ID incompatível.");

            var validationError = Validate(pet);
            if (validationError != null)
                return (false, validationError);

            var existente = await _repository.GetByIdAsync(id);
            if (existente == null)
                return (false, "Pet não encontrado.");

            existente.Name = pet.Name;
            existente.Breed = pet.Breed;
            existente.Age = pet.Age;
            existente.TutorName = pet.TutorName;
            existente.NeedsPostOpCare = pet.NeedsPostOpCare;

            await _repository.UpdateAsync(existente);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Pet {PetId} atualizado com sucesso", id);

            if (_careLogRepository != null)
            {
                try
                {
                    await _careLogRepository.AddAsync(new PetCareLog
                    {
                        PetId = id,
                        PetName = existente.Name,
                        ActionType = "Atualização Cadastral",
                        Notes = $"Cadastro do pet atualizado. Cuidados pós-op: {existente.NeedsPostOpCare}",
                        RegisteredBy = author ?? existente.TutorName,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao gravar log clínico NoSQL para Pet {PetId}", id);
                }
            }

            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id, string? author = null)
        {
            var pet = await _repository.GetByIdAsync(id);
            if (pet == null) return false;

            await _repository.DeleteAsync(pet);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Pet {PetId} removido com sucesso", id);

            if (_careLogRepository != null)
            {
                try
                {
                    await _careLogRepository.AddAsync(new PetCareLog
                    {
                        PetId = id,
                        PetName = pet.Name,
                        ActionType = "Exclusão",
                        Notes = "Registro do pet removido da base de dados.",
                        RegisteredBy = author ?? "Sistema",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao gravar log clínico NoSQL para Pet {PetId}", id);
                }
            }

            return true;
        }

        private static string? Validate(Pet? pet)
        {
            if (pet == null) return "Dados inválidos.";
            if (string.IsNullOrWhiteSpace(pet.Name)) return "O nome do pet é obrigatório.";
            if (pet.Age < 0) return "A idade do pet não pode ser negativa.";
            if (string.IsNullOrWhiteSpace(pet.TutorName)) return "O nome do tutor é obrigatório.";
            return null;
        }
    }
}
