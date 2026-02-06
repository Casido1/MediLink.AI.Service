using MediLink.AI.Service.Agents;
using MediLink.AI.Service.Pluggins;
using MediLink.AI.Service.Services;
using MediLink.AI.Service.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Net.Http.Headers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddKernel()
    .AddGoogleAIGeminiChatCompletion("gemini-2.5-flash", builder.Configuration["Gemini:ApiKey"])
    .AddGoogleAIEmbeddingGenerator("gemini-embedding-001", builder.Configuration["Gemini:ApiKey"]);

builder.Services.AddSingleton<IChatClient>(sp =>
{
    // Get the Gemini service we just registered above
    var chatService = sp.GetRequiredService<IChatCompletionService>();

    // Convert it to the unified IChatClient interface
    return chatService.AsChatClient();
});

builder.Services.AddPineconeVectorStore(builder.Configuration["Pinecone:ApiKey"]);

//Plugins
var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.Plugins.AddFromType<PharmacyPlugin>();
kernelBuilder.Plugins.AddFromType<MedicalKnowledgePlugin>();

//Agents
builder.Services.AddKeyedScoped("Diagnostician", (p, key) =>
    MedicalAgents.CreateDiagnostician(p.GetRequiredService<Kernel>()));
builder.Services.AddKeyedScoped("Pharmacist", (p, key) =>
    MedicalAgents.CreatePharmacist(p.GetRequiredService<Kernel>()));

builder.Services.AddScoped<MedicalIngestionService>();
builder.Services.AddScoped<ConsultationWorkflow>();

builder.Services.AddHttpClient<PharmacyPlugin>(client =>
{
    var openFDACOnfig = builder.Configuration.GetSection("OpenFDA");

    client.BaseAddress = new Uri(openFDACOnfig["BaseUrl"]);

    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("MediLinkCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MediLink AI API v1");

        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("MediLinkCorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
