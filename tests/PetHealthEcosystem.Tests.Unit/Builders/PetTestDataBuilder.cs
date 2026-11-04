using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Tests.Unit.Builders
{
    /// <summary>
    /// Test Data Builder (Object Mother) para criar instâncias da entidade Pet
    /// com dados padronizados e customizáveis para automação de testes de QA.
    /// </summary>
    public class PetTestDataBuilder
    {
        private int _id = 1;
        private string _name = "Rex";
        private string _species = "Canino";
        private string _breed = "Golden Retriever";
        private int _age = 3;
        private decimal _weight = 25.5m;
        private string _size = "Grande";
        private string _tutorName = "Carlos Silva";
        private string _tutorPhone = "+5511999998888";
        private bool _needsPostOpCare = false;
        private string? _surgeryType = null;
        private DateTime? _surgeryDate = null;
        private string _recoveryStatus = "Normal";

        public static PetTestDataBuilder CreateDefault() => new();

        public PetTestDataBuilder WithId(int id) { _id = id; return this; }
        public PetTestDataBuilder WithName(string name) { _name = name; return this; }
        public PetTestDataBuilder WithSpecies(string species) { _species = species; return this; }
        public PetTestDataBuilder WithBreed(string breed) { _breed = breed; return this; }
        public PetTestDataBuilder WithAge(int age) { _age = age; return this; }
        public PetTestDataBuilder WithTutor(string tutorName, string phone = "+5511999998888")
        {
            _tutorName = tutorName;
            _tutorPhone = phone;
            return this;
        }

        public PetTestDataBuilder InPostOpRecovery(string surgery = "Castração")
        {
            _needsPostOpCare = true;
            _surgeryType = surgery;
            _surgeryDate = DateTime.UtcNow.AddDays(-1);
            _recoveryStatus = "Em Recuperação";
            return this;
        }

        public PetTestDataBuilder InCriticalAlert(string surgery = "Cirurgia Ortopédica")
        {
            _needsPostOpCare = true;
            _surgeryType = surgery;
            _surgeryDate = DateTime.UtcNow.AddDays(-2);
            _recoveryStatus = "Alerta Crítico";
            return this;
        }

        public Pet Build()
        {
            return new Pet
            {
                Id = _id,
                Name = _name,
                Species = _species,
                Breed = _breed,
                Age = _age,
                Weight = _weight,
                Size = _size,
                TutorName = _tutorName,
                TutorPhone = _tutorPhone,
                NeedsPostOpCare = _needsPostOpCare,
                SurgeryType = _surgeryType,
                SurgeryDate = _surgeryDate,
                RecoveryStatus = _recoveryStatus
            };
        }
    }
}
