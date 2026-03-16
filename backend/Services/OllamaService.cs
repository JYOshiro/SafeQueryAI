using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Api.Services;

/// <summary>
/// Communicates with a locally running Ollama instance for embeddings and chat generation.
/// Ollama must be installed and running (https://ollama.com).
/// </summary>
public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _embeddingModel;
    private readonly string _generationModel;
    private readonly ILogger<OllamaService> _logger;

    public OllamaService(HttpClient httpClient, IConfiguration config, ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _embeddingModel = config.GetValue<string>("Ollama:EmbeddingModel") ?? "nomic-embed-text";
        _generationModel = config.GetValue<string>("Ollama:GenerationModel") ?? "llama3.2";
        _logger = logger;
    }

    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new { model = _embeddingModel, input = text };

        var response = await _httpClient.PostAsJsonAsync("/api/embed", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(cancellationToken: cancellationToken);
        return result?.Embeddings?[0] ?? Array.Empty<float>();
    }

    public async Task<string> GenerateAnswerAsync(
        string question,
        IEnumerable<string> contextChunks,
        CancellationToken cancellationToken = default)
    {
        var context = string.Join("\n\n---\n\n", contextChunks);

        var systemPrompt =
            "You are a precise document analysis assistant. " +
            "Answer the user's question using ONLY the document excerpts provided below. " +
            "If the answer cannot be found in the excerpts, say so clearly and do not speculate. " +
            "Do not use any knowledge outside of the provided context.\n\n" +
            "Document excerpts:\n" + context;

        var request = new
        {
            model = _generationModel,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = question }
            },
            stream = false
        };

        _logger.LogDebug("Sending question to Ollama model {Model}.", _generationModel);

        var response = await _httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken);
        return result?.Message?.Content ?? "Unable to generate an answer from the model.";
    }

    // ---- Local response DTOs ------------------------------------------------

    private record OllamaEmbedResponse(
        [property: JsonPropertyName("embeddings")] float[][]? Embeddings);

    private record OllamaChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaChatMessage? Message);
}
