using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using MediLink.AI.Service.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using System.Text;

namespace MediLink.AI.Service.Services
{
    public class MedicalIngestionService(
    VectorStore vectorStore, // Provided by Abstractions package
    IEmbeddingGenerator<string, Embedding<float>> embeddingService)
    {
        public async Task IngestManualAsync(Stream pdfStream, string sourceName, CancellationToken ct)
        {
            //Extract text from pdf
            var fullText = new StringBuilder();
            using (var pdfReader = new PdfReader(pdfStream)) // stream from your controller
            using (var pdfDoc = new PdfDocument(pdfReader))
            {
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    try
                    {
                        var page = pdfDoc.GetPage(i);
                        // LocationTextExtractionStrategy handles columns better for manuals
                        var strategy = new LocationTextExtractionStrategy();
                        string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);

                        fullText.AppendLine(pageText);
                    }
                    catch (Exception ex)
                    {
                        // If a page is truly broken, we skip it instead of crashing the app
                        Console.WriteLine($"Could not parse page {i}: {ex.Message}");
                    }
                }
            }

            //Chunking into 1000 characters with an overlap of 200
            var chunks = SplitText(fullText.ToString(), 1000, 200);

            // Get the collection (index) from Pinecone
            var collection = vectorStore.GetCollection<string, MedicalKnowledge>("medilink-index");

            // Create the index if it doesn't exist (Pinecone serverless)
            await collection.EnsureCollectionExistsAsync();

            foreach (var (chunk, index) in chunks.Select((c, i) => (c, i)))
            {
                // Generate the 3072-vector using Gemini
                var embedding = await embeddingService.GenerateAsync(chunk);

                var record = new MedicalKnowledge
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = chunk,
                    SourceName = sourceName,
                    Embedding = embedding.Vector
                };

                await collection.UpsertAsync(record);
            }
        }

        private List<string> SplitText(string text, int size, int overlap)
        {
            var chunks = new List<string>();
            for (int i = 0; i < text.Length; i += (size - overlap))
            {
                if (i + size > text.Length) size = text.Length - i;
                chunks.Add(text.Substring(i, size));
                if (i + size >= text.Length) break;
            }
            return chunks;
        }
    }
}