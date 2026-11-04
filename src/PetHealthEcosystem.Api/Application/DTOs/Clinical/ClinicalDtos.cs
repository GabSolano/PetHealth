using System.ComponentModel.DataAnnotations;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Application.DTOs.Clinical
{
    /// <summary>
    /// Payload do Check-in de Pós-Operatório enviado via WhatsApp / App Mobile do PetHealth.
    /// </summary>
    public class PostOpCheckInRequest
    {
        [Required(ErrorMessage = "A aceitação de água é obrigatória.")]
        public string WaterIntake { get; set; } = "Normal"; // Normal, Baixa, Recusa

        [Required(ErrorMessage = "A aceitação de alimento é obrigatória.")]
        public string FoodIntake { get; set; } = "Normal";  // Normal, Parcial, Recusa

        [Range(0, 10, ErrorMessage = "O nível de dor deve estar entre 0 e 10.")]
        public int PainLevel { get; set; } = 0;

        public bool HasVomiting { get; set; } = false;

        [Required(ErrorMessage = "O estado da incisão/curativo é obrigatório.")]
        public string WoundCondition { get; set; } = "Normal"; // Normal, Sangramento, Secreção, Vermelhidão

        public string Observations { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = "Tutor via WhatsApp";
    }

    /// <summary>
    /// Payload para registro de vacinação preventiva no ecossistema PetHealth.
    /// </summary>
    public class VaccineRegistrationRequest
    {
        [Required(ErrorMessage = "O nome da vacina é obrigatório.")]
        public string VaccineName { get; set; } = string.Empty;

        [Required(ErrorMessage = "A dose é obrigatória.")]
        public string Dose { get; set; } = "1ª Dose";

        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
        public DateTime? NextDueDate { get; set; }
        public string Veterinarian { get; set; } = "Dr. Veterinário";
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Prontuário consolidado do Pet (Relacional + Histórico NoSQL).
    /// </summary>
    public class PetMedicalRecordDto
    {
        public Pet Pet { get; set; } = default!;
        public IEnumerable<PetCareLog> CheckIns { get; set; } = Enumerable.Empty<PetCareLog>();
        public IEnumerable<PetCareLog> Vaccines { get; set; } = Enumerable.Empty<PetCareLog>();
        public IEnumerable<PetCareLog> FullHistory { get; set; } = Enumerable.Empty<PetCareLog>();
        public bool HasActiveCriticalAlert { get; set; }
        public string ClinicalSummary { get; set; } = string.Empty;
    }
}
