namespace SafeQueryAI.Api.Services.Interfaces;

public interface IOllamaService
{
    /// <summary>Returns a vector embedding for the given text using the configured embedding model.</summary>
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the question and retrieved context chunks to the LLM and returns a grounded answer.
    /// </summary>
    Task<string> GenerateAnswerAsync(string question, IEnumerable<string> contextChunks, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams the LLM answer token-by-token using Ollama's streaming API.
    /// </summary>
    IAsyncEnumerable<string> GenerateAnswerStreamAsync(string question, IEnumerable<string> contextChunks, CancellationToken cancellationToken = default);
}
