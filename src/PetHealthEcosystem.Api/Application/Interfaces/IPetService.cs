using PetHealthEcosystem.Api.Application.DTOs.Clinical;
using PetHealthEcosystem.Api.Application.DTOs.Hateoas;
using PetHealthEcosystem.Api.Application.DTOs.Pets;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Application.Interfaces
{
    /// <summary>
    /// Camada de Aplicação: orquestra as regras de negócio clínicas e preventivas do PetHealth.
    /// </summary>
    public interface IPetService
    {
        Task<IEnumerable<Pet>> GetAllAsync();
        Task<PagedResult<Pet>> GetPagedAsync(PetQueryParameters parameters);
        Task<Pet?> GetByIdAsync(int id);
        Task<IEnumerable<Pet>> GetByBreedAsync(string breed);
        Task<IEnumerable<Pet>> GetByPostOpCareAsync(bool needsCare);
        Task<IEnumerable<Pet>> GetByTutorAsync(string tutorName);
        Task<(bool Success, string? Error, Pet? Pet)> CreateAsync(Pet pet, string? author = null);
        Task<(bool Success, string? Error)> UpdateAsync(int id, Pet pet, string? author = null);
        Task<bool> DeleteAsync(int id, string? author = null);

        // Operações especializadas dos pilares Preventivo e Terapêutico (PetHealth)
        Task<(bool Success, string? Error, PetCareLog? CheckInLog)> RegisterPostOpCheckInAsync(int petId, PostOpCheckInRequest request);
        Task<(bool Success, string? Error, PetCareLog? VaccineLog)> RegisterVaccineAsync(int petId, VaccineRegistrationRequest request);
        Task<PetMedicalRecordDto?> GetMedicalRecordAsync(int petId);
        Task<IEnumerable<Pet>> GetCriticalAlertPetsAsync();
    }
}
