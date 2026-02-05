using MediLink.AI.Service.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace MediLink.AI.Service.Services
{
    public class MedicalIngestionService(
    VectorStore vectorStore, // Provided by Abstractions package
    IEmbeddingGenerator<string, Embedding<float>> embeddingService)
    {
        public async Task IngestManualAsync(string content, string category)
        {
            // Get the collection (index) from Pinecone
            var collection = vectorStore.GetCollection<string, MedicalKnowledge>("medilink-index");

            // Create the index if it doesn't exist (Pinecone serverless)
            await collection.EnsureCollectionExistsAsync();

            var paragraphs = content.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);

            // Generate the 768-vector using Gemini
            var embeddings = await embeddingService.GenerateAsync(paragraphs);

            foreach (var embedding in embeddings)
            {
                var record = new MedicalKnowledge
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = content,
                    Category = category,
                    Embedding = embedding.Vector
                };

                await collection.UpsertAsync(record);
            }

        }
    }
}
