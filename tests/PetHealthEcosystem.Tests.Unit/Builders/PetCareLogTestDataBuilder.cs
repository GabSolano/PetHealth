using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Tests.Unit.Builders
{
    /// <summary>
    /// Builder de prontuário e histórico de cuidados clínicos NoSQL para automação de testes de QA.
    /// </summary>
    public class PetCareLogTestDataBuilder
    {
        private string _id = Guid.NewGuid().ToString("N")[..24];
        private int _petId = 1;
        private string _petName = "Rex";
        private string _actionType = "Check-in Pós-Operatório";
        private string _logType = "CheckInPosOp";
        private string _notes = "Check-in de rotina pós-operatório realizado com sucesso.";
        private string _registeredBy = "Tutor via WhatsApp";
        private bool _isCriticalAlert = false;
        private DateTime _createdAt = DateTime.UtcNow;

        public static PetCareLogTestDataBuilder CreateDefault() => new();

        public PetCareLogTestDataBuilder WithPet(int petId, string petName)
        {
            _petId = petId;
            _petName = petName;
            return this;
        }

        public PetCareLogTestDataBuilder AsCriticalAlert()
        {
            _isCriticalAlert = true;
            _notes = "ALERTA: Pet com dor severa e vômito persistente.";
            return this;
        }

        public PetCareLogTestDataBuilder AsVaccine(string vaccineName, DateTime? nextDueDate = null)
        {
            _logType = "Vacinacao";
            _actionType = $"Vacina: {vaccineName}";
            _notes = $"Vacina {vaccineName} aplicada.";
            return this;
        }

        public PetCareLog Build()
        {
            return new PetCareLog
            {
                Id = _id,
                PetId = _petId,
                PetName = _petName,
                ActionType = _actionType,
                LogType = _logType,
                Notes = _notes,
                RegisteredBy = _registeredBy,
                IsCriticalAlert = _isCriticalAlert,
                CreatedAt = _createdAt
            };
        }
    }
}
