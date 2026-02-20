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
        public string SourceName { get; set; }

        [VectorStoreVector(Dimensions: 768, DistanceFunction = DistanceFunction.CosineSimilarity)] // Ollama nomic-embed-text dimension is 768
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
