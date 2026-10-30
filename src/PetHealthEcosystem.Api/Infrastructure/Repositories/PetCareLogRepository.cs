using MongoDB.Driver;
using PetHealthEcosystem.Api.Domain.Interfaces;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Infrastructure.Repositories
{
    public class PetCareLogRepository : IPetCareLogRepository
    {
        private readonly IMongoCollection<PetCareLog> _collection;

        public PetCareLogRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<PetCareLog>("PetCareLogs");
        }

        public async Task<IEnumerable<PetCareLog>> GetAllAsync()
        {
            var filter = Builders<PetCareLog>.Filter.Empty;
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<PetCareLog>> GetByPetIdAsync(int petId)
        {
            var filter = Builders<PetCareLog>.Filter.Eq(x => x.PetId, petId);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<PetCareLog?> GetByIdAsync(string id)
        {
            var filter = Builders<PetCareLog>.Filter.Eq(x => x.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task AddAsync(PetCareLog log)
        {
            await _collection.InsertOneAsync(log);
        }
    }
}
