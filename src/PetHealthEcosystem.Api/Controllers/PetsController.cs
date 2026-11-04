using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHealthEcosystem.Api.Application.Common;
using PetHealthEcosystem.Api.Application.DTOs.Clinical;
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
        /// Retorna a lista de pets em estado de Alerta Crítico no pós-operatório (para triagem veterinária).
        /// </summary>
        [HttpGet("post-op/critical-alerts")]
        [Authorize(Roles = "Admin,Veterinario")]
        public async Task<IActionResult> GetCriticalAlerts()
        {
            var criticalPets = await _petService.GetCriticalAlertPetsAsync();
            var list = criticalPets.Select(p => p.AddHateoasLinks()).ToList();
            return Ok(list);
        }

        /// <summary>
        /// Retorna o prontuário clínico consolidado do Pet (dados cadastrais + histórico NoSQL de vacinas e check-ins).
        /// </summary>
        [HttpGet("{id:int}/medical-record")]
        public async Task<IActionResult> GetMedicalRecord(int id)
        {
            var record = await _petService.GetMedicalRecordAsync(id);
            if (record == null) return NotFound("Pet não encontrado.");
            record.Pet.AddHateoasLinks();
            return Ok(record);
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
        /// Registra um check-in de pós-operatório (enviado via WhatsApp/IA ou App do Tutor).
        /// Avalia automaticamente sintomas e classifica se há Alerta Crítico.
        /// </summary>
        [HttpPost("{id:int}/post-op-checkin")]
        public async Task<IActionResult> RegisterPostOpCheckIn(int id, [FromBody] PostOpCheckInRequest request)
        {
            if (request == null) return BadRequest("Dados do check-in são obrigatórios.");

            var (success, error, log) = await _petService.RegisterPostOpCheckInAsync(id, request);
            if (!success)
            {
                if (error == "Pet não encontrado.") return NotFound(error);
                return BadRequest(error);
            }

            return Ok(new
            {
                Message = log!.IsCriticalAlert 
                    ? "Check-in registrado. ATENÇÃO: Identificado alerta clínico crítico! Veterinário de plantão acionado." 
                    : "Check-in registrado com sucesso. Recuperação em andamento normal.",
                CheckIn = log
            });
        }

        /// <summary>
        /// Registra uma vacina ou profilaxia preventiva para o pet (Requer perfil de Veterinário ou Admin).
        /// </summary>
        [HttpPost("{id:int}/vaccines")]
        [Authorize(Roles = "Admin,Veterinario")]
        public async Task<IActionResult> RegisterVaccine(int id, [FromBody] VaccineRegistrationRequest request)
        {
            if (request == null) return BadRequest("Dados da vacina são obrigatórios.");

            var (success, error, log) = await _petService.RegisterVaccineAsync(id, request);
            if (!success)
            {
                if (error == "Pet não encontrado.") return NotFound(error);
                return BadRequest(error);
            }

            return Ok(new
            {
                Message = "Vacina registrada com sucesso na carteira de vacinação do Pet.",
                VaccineLog = log
            });
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
