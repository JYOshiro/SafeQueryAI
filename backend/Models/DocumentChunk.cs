namespace SafeQueryAI.Api.Models;

/// <summary>
/// A single text chunk from an uploaded document, along with its vector embedding.
/// Stored in memory per-session for similarity search during RAG retrieval.
/// </summary>
public class DocumentChunk
{
    public string SessionId { get; init; } = string.Empty;
    public string FileId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;

    /// <summary>Vector embedding produced by the configured Ollama embedding model.</summary>
    public float[] Embedding { get; set; } = Array.Empty<float>();
}
