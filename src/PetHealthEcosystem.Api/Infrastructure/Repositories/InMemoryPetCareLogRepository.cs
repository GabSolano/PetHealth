using System.Collections.Concurrent;
using PetHealthEcosystem.Api.Domain.Interfaces;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação em memória de IPetCareLogRepository para testes de integração
    /// e fallback quando MongoDB local não estiver ativo.
    /// </summary>
    public class InMemoryPetCareLogRepository : IPetCareLogRepository
    {
        private readonly ConcurrentBag<PetCareLog> _logs = new();

        public Task<IEnumerable<PetCareLog>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<PetCareLog>>(_logs.OrderByDescending(l => l.CreatedAt).ToList());
        }

        public Task<IEnumerable<PetCareLog>> GetByPetIdAsync(int petId)
        {
            return Task.FromResult<IEnumerable<PetCareLog>>(_logs.Where(l => l.PetId == petId).OrderByDescending(l => l.CreatedAt).ToList());
        }

        public Task<PetCareLog?> GetByIdAsync(string id)
        {
            return Task.FromResult(_logs.FirstOrDefault(l => l.Id == id));
        }

        public Task AddAsync(PetCareLog log)
        {
            if (string.IsNullOrEmpty(log.Id))
            {
                log.Id = Guid.NewGuid().ToString("N")[..24];
            }
            _logs.Add(log);
            return Task.CompletedTask;
        }
    }
}
