using Microsoft.EntityFrameworkCore;
using PetHealthEcosystem.Api.Application.DTOs.Pets;
using PetHealthEcosystem.Api.Data;
using PetHealthEcosystem.Api.Domain.Interfaces;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Infrastructure.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly AppDbContext _context;

        public PetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pet>> GetAllAsync() =>
            await _context.Pets.AsNoTracking().ToListAsync();

        public async Task<(IEnumerable<Pet> Items, int TotalCount)> GetPagedAsync(PetQueryParameters parameters)
        {
            var query = _context.Pets.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.Name))
            {
                var name = parameters.Name.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Breed))
            {
                var breed = parameters.Breed.Trim().ToLower();
                query = query.Where(p => p.Breed.ToLower().Contains(breed));
            }

            if (!string.IsNullOrWhiteSpace(parameters.TutorName))
            {
                var tutor = parameters.TutorName.Trim().ToLower();
                query = query.Where(p => p.TutorName.ToLower().Contains(tutor));
            }

            if (parameters.NeedsPostOpCare.HasValue)
            {
                query = query.Where(p => p.NeedsPostOpCare == parameters.NeedsPostOpCare.Value);
            }

            var totalCount = await query.CountAsync();

            var isDesc = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = (parameters.SortBy?.ToLower()) switch
            {
                "id" => isDesc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id),
                "breed" => isDesc ? query.OrderByDescending(p => p.Breed) : query.OrderBy(p => p.Breed),
                "age" => isDesc ? query.OrderByDescending(p => p.Age) : query.OrderBy(p => p.Age),
                "tutor" => isDesc ? query.OrderByDescending(p => p.TutorName) : query.OrderBy(p => p.TutorName),
                _ => isDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
            };

            var skip = (parameters.PageNumber - 1) * parameters.PageSize;
            var items = await query.Skip(skip).Take(parameters.PageSize).ToListAsync();

            return (items, totalCount);
        }

        public async Task<Pet?> GetByIdAsync(int id) =>
            await _context.Pets.FindAsync(id);

        public async Task<IEnumerable<Pet>> GetByBreedAsync(string breed) =>
            await _context.Pets
                .Where(p => p.Breed.ToLower() == breed.ToLower())
                .ToListAsync();

        public async Task<IEnumerable<Pet>> GetByPostOpCareAsync(bool needsCare) =>
            await _context.Pets
                .Where(p => p.NeedsPostOpCare == needsCare)
                .ToListAsync();

        public async Task<IEnumerable<Pet>> GetByTutorAsync(string tutorName) =>
            await _context.Pets
                .Where(p => p.TutorName.Contains(tutorName))
                .ToListAsync();

        public async Task AddAsync(Pet pet) =>
            await _context.Pets.AddAsync(pet);

        public Task UpdateAsync(Pet pet)
        {
            _context.Pets.Update(pet);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Pet pet)
        {
            _context.Pets.Remove(pet);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(int id) =>
            await _context.Pets.AnyAsync(p => p.Id == id);

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
