using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PetHealthEcosystem.Api.Models
{
    /// <summary>
    /// Documento NoSQL armazenado no MongoDB para prontuário clínico digital,
    /// check-ins pós-operatórios (via WhatsApp/App) e histórico vacinal.
    /// </summary>
    public class PetCareLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string ActionType { get; set; } = "Cuidado Clínico";
        public string LogType { get; set; } = "Geral"; // "CheckInPosOp", "Vacinacao", "Profilaxia", "TriagemIA", "Geral"
        public string Notes { get; set; } = string.Empty;
        public string RegisteredBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Pilar Terapêutico / Check-in Pós-Operatório
        public string? WaterIntake { get; set; } // Normal, Baixa, Recusa
        public string? FoodIntake { get; set; }  // Normal, Parcial, Recusa
        public int? PainLevel { get; set; }      // 0 a 10
        public bool? HasVomiting { get; set; }
        public string? WoundCondition { get; set; } // Normal, Sangramento, Secreção, Vermelhidão
        public bool IsCriticalAlert { get; set; } = false;

        // Pilar Preventivo / Vacinação e Profilaxia
        public string? VaccineName { get; set; }
        public string? Dose { get; set; }
        public DateTime? NextDueDate { get; set; }
    }
}
