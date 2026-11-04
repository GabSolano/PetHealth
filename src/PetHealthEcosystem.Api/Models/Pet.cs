using PetHealthEcosystem.Api.Application.DTOs.Hateoas;

namespace PetHealthEcosystem.Api.Models
{
    /// <summary>
    /// Entidade principal de domínio que representa o Pet no ecossistema PetHealth.
    /// Contempla dados cadastrais, canal de contato com o tutor e histórico clínico/pós-operatório.
    /// </summary>
    public class Pet
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = "Canino"; // Canino, Felino, etc.
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }
        public decimal Weight { get; set; } = 10.0m;
        public string Size { get; set; } = "Médio"; // Pequeno, Médio, Grande

        // Dados de contato para o canal omnichannel / WhatsApp com a IA
        public string TutorName { get; set; } = string.Empty;
        public string TutorPhone { get; set; } = string.Empty;

        // Pilar Terapêutico e Pós-Operatório
        public bool NeedsPostOpCare { get; set; }
        public string? SurgeryType { get; set; } // ex: Castração, Ortopédica, Profilaxia dental
        public DateTime? SurgeryDate { get; set; }
        public string RecoveryStatus { get; set; } = "Normal"; // Normal, Em Observação, Alerta Crítico, Recuperado

        public List<LinkDto> Links { get; set; } = new();
    }
}
