using Microsoft.Extensions.Logging;
using PetHealthEcosystem.Api.Application.DTOs.Clinical;
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
            _logger.LogInformation("Consultando todos os pets cadastrados no ecossistema PetHealth");
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

            _logger.LogInformation("Pet {PetId} ({PetName}) cadastrado com sucesso", pet.Id, pet.Name);

            if (_careLogRepository != null)
            {
                try
                {
                    await _careLogRepository.AddAsync(new PetCareLog
                    {
                        PetId = pet.Id,
                        PetName = pet.Name,
                        ActionType = "Cadastro",
                        LogType = "Geral",
                        Notes = $"Pet cadastrado na plataforma PetHealth. Espécie: {pet.Species}, Porte: {pet.Size}, Peso: {pet.Weight}kg, Pós-Op: {pet.NeedsPostOpCare}",
                        RegisteredBy = author ?? pet.TutorName,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao gravar log clínico inicial no MongoDB para Pet {PetId}", pet.Id);
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
            existente.Species = pet.Species;
            existente.Breed = pet.Breed;
            existente.Age = pet.Age;
            existente.Weight = pet.Weight;
            existente.Size = pet.Size;
            existente.TutorName = pet.TutorName;
            existente.TutorPhone = pet.TutorPhone;
            existente.NeedsPostOpCare = pet.NeedsPostOpCare;
            existente.SurgeryType = pet.SurgeryType;
            existente.SurgeryDate = pet.SurgeryDate;
            existente.RecoveryStatus = pet.RecoveryStatus;

            await _repository.UpdateAsync(existente);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Dados cadastrais do Pet {PetId} atualizados com sucesso", id);

            if (_careLogRepository != null)
            {
                try
                {
                    await _careLogRepository.AddAsync(new PetCareLog
                    {
                        PetId = id,
                        PetName = existente.Name,
                        ActionType = "Atualização Cadastral",
                        LogType = "Geral",
                        Notes = $"Cadastro atualizado. Status de Recuperação: {existente.RecoveryStatus}",
                        RegisteredBy = author ?? existente.TutorName,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao gravar log clínico no MongoDB para Pet {PetId}", id);
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

            _logger.LogInformation("Pet {PetId} removido do ecossistema", id);

            if (_careLogRepository != null)
            {
                try
                {
                    await _careLogRepository.AddAsync(new PetCareLog
                    {
                        PetId = id,
                        PetName = pet.Name,
                        ActionType = "Exclusão",
                        LogType = "Geral",
                        Notes = "Registro do pet removido da base de dados.",
                        RegisteredBy = author ?? "Sistema",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao gravar log de exclusão no MongoDB para Pet {PetId}", id);
                }
            }

            return true;
        }

        // =====================================================================
        // PILARES PREVENTIVO E TERAPÊUTICO (PETHEALTH CORE)
        // =====================================================================

        public async Task<(bool Success, string? Error, PetCareLog? CheckInLog)> RegisterPostOpCheckInAsync(int petId, PostOpCheckInRequest request)
        {
            var pet = await _repository.GetByIdAsync(petId);
            if (pet == null)
                return (false, "Pet não encontrado.", null);

            // Regra Clínica PetHealth: Avaliação de Alerta Crítico
            var isWoundCritical = request.WoundCondition.Equals("Sangramento", StringComparison.OrdinalIgnoreCase) ||
                                  request.WoundCondition.Equals("Secreção", StringComparison.OrdinalIgnoreCase);
            var isPainHigh = request.PainLevel >= 7;
            var isIntakeRefused = request.WaterIntake.Equals("Recusa", StringComparison.OrdinalIgnoreCase) &&
                                  request.FoodIntake.Equals("Recusa", StringComparison.OrdinalIgnoreCase);

            var isCritical = isWoundCritical || isPainHigh || request.HasVomiting || isIntakeRefused;

            if (isCritical)
            {
                pet.RecoveryStatus = "Alerta Crítico";
                _logger.LogWarning("⚠️ ALERTA CRÍTICO DE PÓS-OPERATÓRIO para Pet {PetId} ({PetName})! Notificando veterinário de plantão.", pet.Id, pet.Name);
            }
            else
            {
                pet.RecoveryStatus = "Em Recuperação";
            }

            await _repository.UpdateAsync(pet);
            await _repository.SaveChangesAsync();

            var careLog = new PetCareLog
            {
                PetId = pet.Id,
                PetName = pet.Name,
                ActionType = "Check-in Pós-Operatório",
                LogType = "CheckInPosOp",
                Notes = $"Check-in: Água={request.WaterIntake}, Alimento={request.FoodIntake}, Dor={request.PainLevel}/10, Vômito={request.HasVomiting}, Incisão={request.WoundCondition}. Obs: {request.Observations}",
                RegisteredBy = request.SubmittedBy,
                WaterIntake = request.WaterIntake,
                FoodIntake = request.FoodIntake,
                PainLevel = request.PainLevel,
                HasVomiting = request.HasVomiting,
                WoundCondition = request.WoundCondition,
                IsCriticalAlert = isCritical,
                CreatedAt = DateTime.UtcNow
            };

            if (_careLogRepository != null)
            {
                await _careLogRepository.AddAsync(careLog);
            }

            return (true, null, careLog);
        }

        public async Task<(bool Success, string? Error, PetCareLog? VaccineLog)> RegisterVaccineAsync(int petId, VaccineRegistrationRequest request)
        {
            var pet = await _repository.GetByIdAsync(petId);
            if (pet == null)
                return (false, "Pet não encontrado.", null);

            if (string.IsNullOrWhiteSpace(request.VaccineName))
                return (false, "O nome da vacina é obrigatório.", null);

            var nextDueDate = request.NextDueDate ?? request.ApplicationDate.AddDays(365); // Reforço anual padrão

            var vaccineLog = new PetCareLog
            {
                PetId = pet.Id,
                PetName = pet.Name,
                ActionType = $"Vacinação: {request.VaccineName}",
                LogType = "Vacinacao",
                Notes = $"Vacina {request.VaccineName} ({request.Dose}) aplicada. Próximo reforço previsto em: {nextDueDate:dd/MM/yyyy}. Obs: {request.Notes}",
                RegisteredBy = request.Veterinarian,
                VaccineName = request.VaccineName,
                Dose = request.Dose,
                NextDueDate = nextDueDate,
                CreatedAt = DateTime.UtcNow
            };

            if (_careLogRepository != null)
            {
                await _careLogRepository.AddAsync(vaccineLog);
            }

            _logger.LogInformation("Vacina {Vaccine} registrada para Pet {PetId}. Próximo reforço: {DueDate:yyyy-MM-dd}", request.VaccineName, pet.Id, nextDueDate);
            return (true, null, vaccineLog);
        }

        public async Task<PetMedicalRecordDto?> GetMedicalRecordAsync(int petId)
        {
            var pet = await _repository.GetByIdAsync(petId);
            if (pet == null) return null;

            var allLogs = _careLogRepository != null 
                ? await _careLogRepository.GetByPetIdAsync(petId) 
                : Enumerable.Empty<PetCareLog>();

            var logsList = allLogs.ToList();
            var checkIns = logsList.Where(l => l.LogType == "CheckInPosOp").OrderByDescending(l => l.CreatedAt).ToList();
            var vaccines = logsList.Where(l => l.LogType == "Vacinacao").OrderByDescending(l => l.CreatedAt).ToList();

            var hasActiveCriticalAlert = checkIns.Any(c => c.IsCriticalAlert) || pet.RecoveryStatus == "Alerta Crítico";

            return new PetMedicalRecordDto
            {
                Pet = pet,
                CheckIns = checkIns,
                Vaccines = vaccines,
                FullHistory = logsList,
                HasActiveCriticalAlert = hasActiveCriticalAlert,
                ClinicalSummary = $"Pet {pet.Name} ({pet.Species}, {pet.Breed}, {pet.Age} anos). Status: {pet.RecoveryStatus}. Total de check-ins pós-op: {checkIns.Count}, Vacinas: {vaccines.Count}."
            };
        }

        public async Task<IEnumerable<Pet>> GetCriticalAlertPetsAsync()
        {
            _logger.LogInformation("Consultando pets em alerta crítico de pós-operatório");
            return await _repository.GetCriticalAlertsAsync();
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
