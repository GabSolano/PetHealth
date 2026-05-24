using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetHealthEcosystem.Api.Data;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PetsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var pets = await _context.Pets.ToListAsync();
            return Ok(pets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound();
            return Ok(pet);
        }

        [HttpGet("breed/{breed}")]
        public async Task<IActionResult> GetByBreed(string breed)
        {
            var pets = await _context.Pets.Where(p => p.Breed.ToLower() == breed.ToLower()).ToListAsync();
            if (!pets.Any()) return NotFound("Nenhum pet dessa raça encontrado.");
            return Ok(pets);
        }

        [HttpGet("post-op/{needsCare}")]
        public async Task<IActionResult> GetByPostOpCare(bool needsCare)
        {
            var pets = await _context.Pets.Where(p => p.NeedsPostOpCare == needsCare).ToListAsync();
            return Ok(pets);
        }

        [HttpGet("tutor/{tutorName}")]
        public async Task<IActionResult> GetByTutor(string tutorName)
        {
            var pets = await _context.Pets.Where(p => p.TutorName.Contains(tutorName)).ToListAsync();
            if (!pets.Any()) return NotFound("Tutor não encontrado.");
            return Ok(pets);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Pet pet)
        {
            if (pet == null || string.IsNullOrWhiteSpace(pet.Name))
                return BadRequest("Dados inválidos.");

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Pet petAtualizado)
        {
            if (id != petAtualizado.Id) return BadRequest("ID incompatível.");

            var petExistente = await _context.Pets.FindAsync(id);
            if (petExistente == null) return NotFound();

            petExistente.Name = petAtualizado.Name;
            petExistente.Breed = petAtualizado.Breed;
            petExistente.Age = petAtualizado.Age;
            petExistente.TutorName = petAtualizado.TutorName;
            petExistente.NeedsPostOpCare = petAtualizado.NeedsPostOpCare;

            _context.Pets.Update(petExistente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return NotFound();

            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}