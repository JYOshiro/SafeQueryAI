using SafeQueryAI.Api.Services;
using SafeQueryAI.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register core application services
builder.Services.AddSingleton<ISessionService, SessionService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<ITextExtractionService, TextExtractionService>();

// Register RAG services
builder.Services.AddSingleton<IVectorStoreService, VectorStoreService>();
builder.Services.AddSingleton<IDocumentIndexingService, DocumentIndexingService>();
builder.Services.AddSingleton<IQuestionAnsweringService, QuestionAnsweringService>();

// Background service that expires abandoned sessions and removes their data,
// enforcing the temporary-session privacy promise.
builder.Services.AddHostedService<SessionExpiryService>();

// Register Ollama HTTP client — local-only enforcement
var ollamaBaseUrl = builder.Configuration.GetValue<string>("Ollama:BaseUrl") ?? "http://localhost:11434";

// PRIVACY GUARDRAIL: Reject any Ollama URL that is not a local loopback address.
// This prevents accidental or deliberate routing of document content to cloud endpoints.
if (!Uri.TryCreate(ollamaBaseUrl, UriKind.Absolute, out var ollamaUri)
    || !ollamaUri.IsLoopback)
{
    throw new InvalidOperationException(
        $"PRIVACY VIOLATION: Ollama:BaseUrl '{ollamaBaseUrl}' is not a local loopback address. " +
        "SafeQueryAI only permits local Ollama inference (e.g. http://localhost:11434). " +
        "Document content must never leave the local environment.");
}

builder.Services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
    client.BaseAddress = ollamaUri;
    // LLM generation can take time, especially on first token
    client.Timeout = TimeSpan.FromMinutes(5);
});

// Allow CORS for local frontend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("LocalDev");
app.UseAuthorization();
app.MapControllers();

app.Run();
