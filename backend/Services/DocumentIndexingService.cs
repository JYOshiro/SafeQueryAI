using System.Text;
using SafeQueryAI.Api.Models;
using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Api.Services;

/// <summary>
/// Orchestrates the RAG indexing pipeline: text chunking → embedding → vector store insertion.
/// </summary>
public class DocumentIndexingService : IDocumentIndexingService
{
    // Target character length of each chunk
    private const int ChunkSize = 600;
    // Overlap carried from the end of one chunk to the start of the next
    private const int ChunkOverlap = 100;

    private readonly IOllamaService _ollama;
    private readonly IVectorStoreService _vectorStore;
    private readonly ILogger<DocumentIndexingService> _logger;

    public DocumentIndexingService(
        IOllamaService ollama,
        IVectorStoreService vectorStore,
        ILogger<DocumentIndexingService> logger)
    {
        _ollama = ollama;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    public async Task IndexFileAsync(
        string sessionId,
        string fileId,
        string fileName,
        string extractedText,
        CancellationToken cancellationToken = default)
    {
        var rawChunks = ChunkText(extractedText);
        _logger.LogInformation("Indexing {FileName}: {Count} chunks to embed.", fileName, rawChunks.Count);

        var documentChunks = new List<DocumentChunk>(rawChunks.Count);

        foreach (var chunkText in rawChunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var embedding = await _ollama.GetEmbeddingAsync(chunkText, cancellationToken);
            documentChunks.Add(new DocumentChunk
            {
                SessionId = sessionId,
                FileId    = fileId,
                FileName  = fileName,
                Text      = chunkText,
                Embedding = embedding
            });
        }

        _vectorStore.AddChunks(documentChunks);
        _logger.LogInformation("Indexed {Count} chunks for {FileName}.", documentChunks.Count, fileName);
    }

    public void RemoveFileIndex(string sessionId, string fileId) =>
        _vectorStore.RemoveFile(sessionId, fileId);

    public void RemoveSessionIndex(string sessionId) =>
        _vectorStore.RemoveSession(sessionId);

    // -------------------------------------------------------------------------
    // Text chunking: paragraph-aware with overlap
    // -------------------------------------------------------------------------

    private static List<string> ChunkText(string text)
    {
        var chunks = new List<string>();
        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        // Split on blank lines to get natural paragraphs
        var paragraphs = text
            .Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();

        var current = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            // Long single paragraph: hard-split it
            if (paragraph.Length > ChunkSize)
            {
                if (current.Length > 0)
                {
                    chunks.Add(current.ToString().Trim());
                    current.Clear();
                }

                for (int i = 0; i < paragraph.Length; i += ChunkSize - ChunkOverlap)
                {
                    var end = Math.Min(i + ChunkSize, paragraph.Length);
                    chunks.Add(paragraph[i..end].Trim());
                    if (end >= paragraph.Length) break;
                }
                continue;
            }

            // Adding this paragraph would overflow the current chunk
            if (current.Length + paragraph.Length > ChunkSize && current.Length > 0)
            {
                var flushed = current.ToString();
                chunks.Add(flushed.Trim());

                // Carry overlap into next chunk
                current.Clear();
                if (flushed.Length > ChunkOverlap)
                    current.Append(flushed[^ChunkOverlap..]);
                else
                    current.Append(flushed);
            }

            current.Append(paragraph).Append("\n\n");
        }

        if (current.Length > 0)
            chunks.Add(current.ToString().Trim());

        return chunks;
    }
}
