using Microsoft.SemanticKernel;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddKernel()
    .AddGoogleAIGeminiChatCompletion("gemini-1.5-flash", builder.Configuration["Gemini:ApiKey"])
    .AddGoogleAIEmbeddingGenerator("text-embedding-004", builder.Configuration["Gemini:ApiKey"]);

builder.Services.AddPineconeVectorStore(builder.Configuration["Pinecone:ApiKey"]);

//kernelBuilder.Plugins.AddFromType<PharmacyPlugin>();

//builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
