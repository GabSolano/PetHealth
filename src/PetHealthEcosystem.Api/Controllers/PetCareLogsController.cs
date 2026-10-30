using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetHealthEcosystem.Api.Application.Interfaces;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetCareLogsController : ControllerBase
    {
        private readonly IPetCareLogService _careLogService;

        public PetCareLogsController(IPetCareLogService careLogService)
        {
            _careLogService = careLogService;
        }

        /// <summary>
        /// Retorna todos os registros NoSQL (MongoDB) de prontuário e cuidados clínicos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _careLogService.GetAllAsync();
            return Ok(logs);
        }

        /// <summary>
        /// Retorna os logs clínicos NoSQL filtrados por PetId.
        /// </summary>
        [HttpGet("pet/{petId:int}")]
        public async Task<IActionResult> GetByPetId(int petId)
        {
            var logs = await _careLogService.GetByPetIdAsync(petId);
            return Ok(logs);
        }

        /// <summary>
        /// Retorna um log clínico específico por Id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var log = await _careLogService.GetByIdAsync(id);
            if (log == null) return NotFound();
            return Ok(log);
        }

        /// <summary>
        /// Registra um novo log clínico ou prontuário NoSQL (Requer autenticação).
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] PetCareLog careLog)
        {
            if (careLog == null) return BadRequest("Dados inválidos.");
            var created = await _careLogService.CreateAsync(careLog);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
    }
}
