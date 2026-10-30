using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PetHealthEcosystem.Api.Models
{
    /// <summary>
    /// Documento NoSQL armazenado no MongoDB para histórico clínico e auditoria de cuidados do Pet.
    /// </summary>
    public class PetCareLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string RegisteredBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
