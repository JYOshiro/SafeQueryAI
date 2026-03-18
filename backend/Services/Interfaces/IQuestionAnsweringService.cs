using SafeQueryAI.Api.Contracts;
using SafeQueryAI.Api.Models;

namespace SafeQueryAI.Api.Services.Interfaces;

public interface IQuestionAnsweringService
{
    /// <summary>
    /// Answers the question using RAG: embeds the question, retrieves the most relevant
    /// document chunks from the session's vector store, and generates a grounded answer
    /// via the configured Ollama LLM. Falls back to keyword matching if Ollama is unavailable.
    /// </summary>
    Task<AskQuestionResponse> AnswerAsync(
        string question,
        string sessionId,
        IReadOnlyList<StoredFileInfo> files,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams the answer token-by-token. Yields <see cref="AnswerStreamChunk"/> items where
    /// each item is either a partial text token or the final metadata event.
    /// Falls back to keyword matching (single token) when Ollama is unavailable.
    /// </summary>
    IAsyncEnumerable<AnswerStreamChunk> StreamAnswerAsync(
        string question,
        string sessionId,
        IReadOnlyList<StoredFileInfo> files,
        CancellationToken cancellationToken = default);
}
