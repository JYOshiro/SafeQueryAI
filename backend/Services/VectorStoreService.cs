using System.Collections.Concurrent;
using SafeQueryAI.Api.Models;
using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Api.Services;

/// <summary>
/// In-memory vector store, scoped per session.
/// Similarity search uses cosine similarity over float embeddings.
/// </summary>
public class VectorStoreService : IVectorStoreService
{
    // sessionId -> list of embedded chunks
    private readonly ConcurrentDictionary<string, List<DocumentChunk>> _store = new();

    public void AddChunks(IEnumerable<DocumentChunk> chunks)
    {
        foreach (var chunk in chunks)
        {
            var bucket = _store.GetOrAdd(chunk.SessionId, _ => new List<DocumentChunk>());
            lock (bucket)
            {
                bucket.Add(chunk);
            }
        }
    }

    public IReadOnlyList<DocumentChunk> Search(string sessionId, float[] queryEmbedding, int topK = 5)
    {
        if (!_store.TryGetValue(sessionId, out var bucket))
            return Array.Empty<DocumentChunk>();

        List<DocumentChunk> snapshot;
        lock (bucket)
        {
            snapshot = bucket.ToList();
        }

        return snapshot
            .Select(c => (Chunk: c, Score: CosineSimilarity(queryEmbedding, c.Embedding)))
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Chunk)
            .ToList();
    }

    public void RemoveFile(string sessionId, string fileId)
    {
        if (_store.TryGetValue(sessionId, out var bucket))
        {
            lock (bucket)
            {
                bucket.RemoveAll(c => c.FileId == fileId);
            }
        }
    }

    public void RemoveSession(string sessionId) =>
        _store.TryRemove(sessionId, out _);

    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0)
            return 0f;

        float dot = 0f, magA = 0f, magB = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            dot  += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        var denom = Math.Sqrt(magA) * Math.Sqrt(magB);
        return denom < 1e-10 ? 0f : (float)(dot / denom);
    }
}
