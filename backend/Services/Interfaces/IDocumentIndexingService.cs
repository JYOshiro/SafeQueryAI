namespace SafeQueryAI.Api.Services.Interfaces;

public interface IDocumentIndexingService
{
    /// <summary>
    /// Splits the extracted text into chunks, embeds each chunk via Ollama,
    /// and stores the results in the vector store for later similarity search.
    /// </summary>
    Task IndexFileAsync(
        string sessionId,
        string fileId,
        string fileName,
        string extractedText,
        CancellationToken cancellationToken = default);

    /// <summary>Removes all indexed chunks for a specific file in a session.</summary>
    void RemoveFileIndex(string sessionId, string fileId);

    /// <summary>Removes all indexed chunks for the entire session.</summary>
    void RemoveSessionIndex(string sessionId);
}
