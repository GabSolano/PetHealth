using PetHealthEcosystem.Api.Application.DTOs.Clinical;

namespace PetHealthEcosystem.Tests.Unit.Builders
{
    /// <summary>
    /// Builder de payloads de Check-in Pós-Operatório para automação de testes de QA.
    /// Facilita a criação de cenários de recuperação saudável e alertas críticos.
    /// </summary>
    public class PostOpCheckInBuilder
    {
        private string _waterIntake = "Normal";
        private string _foodIntake = "Normal";
        private int _painLevel = 1;
        private bool _hasVomiting = false;
        private string _woundCondition = "Normal";
        private string _observations = "Pet ativo e tranquilo";
        private string _submittedBy = "Tutor via WhatsApp";

        public static PostOpCheckInBuilder CreateHealthy() => new();

        public static PostOpCheckInBuilder CreateCritical()
        {
            return new PostOpCheckInBuilder()
                .WithSeverePain(8)
                .WithWoundBleeding()
                .WithVomiting(true);
        }

        public PostOpCheckInBuilder WithWaterIntake(string status) { _waterIntake = status; return this; }
        public PostOpCheckInBuilder WithFoodIntake(string status) { _foodIntake = status; return this; }
        public PostOpCheckInBuilder WithSeverePain(int level = 8) { _painLevel = level; return this; }
        public PostOpCheckInBuilder WithVomiting(bool vomiting = true) { _hasVomiting = vomiting; return this; }
        public PostOpCheckInBuilder WithWoundBleeding() { _woundCondition = "Sangramento"; return this; }
        public PostOpCheckInBuilder WithWoundSecretion() { _woundCondition = "Secreção"; return this; }

        public PostOpCheckInRequest Build()
        {
            return new PostOpCheckInRequest
            {
                WaterIntake = _waterIntake,
                FoodIntake = _foodIntake,
                PainLevel = _painLevel,
                HasVomiting = _hasVomiting,
                WoundCondition = _woundCondition,
                Observations = _observations,
                SubmittedBy = _submittedBy
            };
        }
    }
}
