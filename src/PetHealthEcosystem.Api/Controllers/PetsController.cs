using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHealthEcosystem.Api.Application.Common;
using PetHealthEcosystem.Api.Application.DTOs.Pets;
using PetHealthEcosystem.Api.Application.Interfaces;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetsController : ControllerBase
    {
        private readonly IPetService _petService;
        private readonly ILogger<PetsController> _logger;

        public PetsController(IPetService petService, ILogger<PetsController> logger)
        {
            _petService = petService;
            _logger = logger;
        }

        /// <summary>
        /// Retorna a lista de pets com suporte a paginação, ordenação e filtros dinâmicos, com HATEOAS.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PetQueryParameters parameters)
        {
            var pagedResult = await _petService.GetPagedAsync(parameters);
            pagedResult.AddHateoasLinks();
            return Ok(pagedResult);
        }

        /// <summary>
        /// Retorna os detalhes de um pet específico pelo Id, com links HATEOAS.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pet = await _petService.GetByIdAsync(id);
            if (pet == null) return NotFound();

            pet.AddHateoasLinks();
            return Ok(pet);
        }

        /// <summary>
        /// Retorna pets filtrados por raça.
        /// </summary>
        [HttpGet("breed/{breed}")]
        public async Task<IActionResult> GetByBreed(string breed)
        {
            var pets = await _petService.GetByBreedAsync(breed);
            var list = pets.Select(p => p.AddHateoasLinks()).ToList();
            if (!list.Any()) return NotFound("Nenhum pet dessa raça encontrado.");
            return Ok(list);
        }

        /// <summary>
        /// Retorna pets que necessitam ou não de cuidados pós-operatórios.
        /// </summary>
        [HttpGet("post-op/{needsCare:bool}")]
        public async Task<IActionResult> GetByPostOpCare(bool needsCare)
        {
            var pets = await _petService.GetByPostOpCareAsync(needsCare);
            var list = pets.Select(p => p.AddHateoasLinks()).ToList();
            return Ok(list);
        }

        /// <summary>
        /// Retorna pets pelo nome do tutor.
        /// </summary>
        [HttpGet("tutor/{tutorName}")]
        public async Task<IActionResult> GetByTutor(string tutorName)
        {
            var pets = await _petService.GetByTutorAsync(tutorName);
            var list = pets.Select(p => p.AddHateoasLinks()).ToList();
            if (!list.Any()) return NotFound("Tutor não encontrado.");
            return Ok(list);
        }

        /// <summary>
        /// Cadastra um novo pet no ecossistema (Requer autenticação JWT).
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] Pet pet)
        {
            if (pet == null)
                return BadRequest("Dados inválidos.");

            var author = User.Identity?.Name ?? pet.TutorName;
            var (success, error, created) = await _petService.CreateAsync(pet, author);
            if (!success) return BadRequest(error);

            created!.AddHateoasLinks();
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Atualiza os dados de um pet existente (Requer autenticação JWT).
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] Pet petAtualizado)
        {
            var author = User.Identity?.Name;
            var (success, error) = await _petService.UpdateAsync(id, petAtualizado, author);
            if (!success)
            {
                if (error == "Pet não encontrado.") return NotFound(error);
                return BadRequest(error);
            }

            return NoContent();
        }

        /// <summary>
        /// Remove um pet do ecossistema (Requer permissão de Admin).
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var author = User.Identity?.Name;
            var deleted = await _petService.DeleteAsync(id, author);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
