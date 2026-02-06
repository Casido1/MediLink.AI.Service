using Microsoft.Extensions.VectorData;

namespace MediLink.AI.Service.Models
{
    public record MedicalKnowledge
    {
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [VectorStoreData]
        public string Text { get; set; }

        [VectorStoreData]
        public string Category { get; set; } // e.g., "Pharmacology", "Diagnosis"

        [VectorStoreVector(Dimensions: 3072, DistanceFunction = DistanceFunction.CosineSimilarity)] // Gemini embedding dimension is 3072
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
