using SafeQueryAI.Api.Models;

namespace SafeQueryAI.Api.Services.Interfaces;

public interface IVectorStoreService
{
    /// <summary>Persists a batch of embedded document chunks into the in-memory store.</summary>
    void AddChunks(IEnumerable<DocumentChunk> chunks);

    /// <summary>
    /// Returns the top-k chunks from the session whose embeddings are most similar
    /// to the provided query embedding (cosine similarity).
    /// </summary>
    IReadOnlyList<DocumentChunk> Search(string sessionId, float[] queryEmbedding, int topK = 5);

    /// <summary>Removes all chunks belonging to a specific file within a session.</summary>
    void RemoveFile(string sessionId, string fileId);

    /// <summary>Removes all chunks for every file in the session.</summary>
    void RemoveSession(string sessionId);
}
