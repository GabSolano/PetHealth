using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Application.Interfaces
{
    public interface IPetCareLogService
    {
        Task<IEnumerable<PetCareLog>> GetAllAsync();
        Task<IEnumerable<PetCareLog>> GetByPetIdAsync(int petId);
        Task<PetCareLog?> GetByIdAsync(string id);
        Task<PetCareLog> CreateAsync(PetCareLog log);
    }
}
