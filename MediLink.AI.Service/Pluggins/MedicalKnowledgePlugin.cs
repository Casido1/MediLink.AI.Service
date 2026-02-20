using MediLink.AI.Service.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace MediLink.AI.Service.Pluggins
{
    public class MedicalKnowledgePlugin(
    VectorStore vectorStore, // Provided by Abstractions package
    IEmbeddingGenerator<string, Embedding<float>> embeddingService)
    {
        private readonly VectorStoreCollection<string, MedicalKnowledge> _collection =
            vectorStore.GetCollection<string, MedicalKnowledge>("medilink-index2");

        [KernelFunction, Description("Medical for medical knowledge based on a query.")]
        public async Task<string> SearchKnowledgeAsync([Description("The query to search for")] string query)
        {
            var embedding = await embeddingService.GenerateAsync(query);

            var searchOptions = new VectorSearchOptions<MedicalKnowledge>
            {
                VectorProperty = x => x.Embedding,
                IncludeVectors = false
            };

            var results = _collection.SearchAsync(embedding, top: 3, options: searchOptions);

            var snippets = new List<string>();

            await foreach (var result in results)
            {
                snippets.Add(result.Record.Text);
            }

            return string.Join("\n---\n", snippets);

        }
    }
}
