using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Domain.Interfaces
{
    /// <summary>
    /// Contrato de persistência NoSQL (MongoDB) para registros de cuidados e prontuário de pets.
    /// </summary>
    public interface IPetCareLogRepository
    {
        Task<IEnumerable<PetCareLog>> GetAllAsync();
        Task<IEnumerable<PetCareLog>> GetByPetIdAsync(int petId);
        Task<PetCareLog?> GetByIdAsync(string id);
        Task AddAsync(PetCareLog log);
    }
}
