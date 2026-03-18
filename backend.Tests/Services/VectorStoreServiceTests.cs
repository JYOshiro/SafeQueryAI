using SafeQueryAI.Api.Models;
using SafeQueryAI.Api.Services;

namespace SafeQueryAI.Tests.Services;

public class VectorStoreServiceTests
{
    private readonly VectorStoreService _sut = new();

    // ── AddChunks / Search ────────────────────────────────────────────────────

    [Fact]
    public void Search_ReturnsEmpty_WhenNoChunksForSession()
    {
        var result = _sut.Search("s1", new float[] { 1f, 0f }, 5);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Search_ReturnsTopKMostSimilarChunks()
    {
        _sut.AddChunks(new[]
        {
            MakeChunk("s1", "f1", "chunk-A", new float[] { 1f, 0f, 0f }),
            MakeChunk("s1", "f1", "chunk-B", new float[] { 0f, 1f, 0f }),
            MakeChunk("s1", "f1", "chunk-C", new float[] { 0f, 0f, 1f }),
        });

        // Query most similar to chunk-A
        var results = _sut.Search("s1", new float[] { 1f, 0f, 0f }, topK: 1);

        results.Should().HaveCount(1);
        results[0].Text.Should().Be("chunk-A");
    }

    [Fact]
    public void Search_ReturnsAtMostTopKChunks()
    {
        _sut.AddChunks(Enumerable.Range(0, 10).Select(i =>
            MakeChunk("s1", "f1", $"chunk-{i}", new float[] { i * 0.1f, 0f })));

        var results = _sut.Search("s1", new float[] { 1f, 0f }, topK: 3);

        results.Should().HaveCount(3);
    }

    [Fact]
    public void Search_DoesNotReturnChunksFromOtherSessions()
    {
        _sut.AddChunks(new[] { MakeChunk("sessionA", "f1", "text", new float[] { 1f, 0f }) });

        var results = _sut.Search("sessionB", new float[] { 1f, 0f }, topK: 5);

        results.Should().BeEmpty();
    }

    // ── RemoveFile ────────────────────────────────────────────────────────────

    [Fact]
    public void RemoveFile_RemovesOnlyChunksForThatFile()
    {
        _sut.AddChunks(new[]
        {
            MakeChunk("s1", "fileA", "text A", new float[] { 1f, 0f }),
            MakeChunk("s1", "fileB", "text B", new float[] { 0f, 1f }),
        });

        _sut.RemoveFile("s1", "fileA");

        var results = _sut.Search("s1", new float[] { 1f, 1f }, topK: 5);
        results.Should().OnlyContain(c => c.FileId == "fileB");
    }

    [Fact]
    public void RemoveFile_DoesNotThrow_WhenSessionNotFound()
    {
        var act = () => _sut.RemoveFile("nonexistent", "f1");

        act.Should().NotThrow();
    }

    // ── RemoveSession ─────────────────────────────────────────────────────────

    [Fact]
    public void RemoveSession_ClearsAllChunksForSession()
    {
        _sut.AddChunks(new[] { MakeChunk("s1", "f1", "text", new float[] { 1f, 0f }) });

        _sut.RemoveSession("s1");

        _sut.Search("s1", new float[] { 1f, 0f }, topK: 5).Should().BeEmpty();
    }

    [Fact]
    public void RemoveSession_DoesNotAffectOtherSessions()
    {
        _sut.AddChunks(new[]
        {
            MakeChunk("s1", "f1", "text-s1", new float[] { 1f, 0f }),
            MakeChunk("s2", "f1", "text-s2", new float[] { 1f, 0f }),
        });

        _sut.RemoveSession("s1");

        _sut.Search("s2", new float[] { 1f, 0f }, topK: 5).Should().HaveCount(1);
    }

    // ── Cosine edge cases ─────────────────────────────────────────────────────

    [Fact]
    public void Search_ReturnsZeroScore_WhenEmbeddingIsAllZeros()
    {
        _sut.AddChunks(new[] { MakeChunk("s1", "f1", "text", new float[] { 0f, 0f }) });

        // Should not throw on zero-vector magnitudes
        var act = () => _sut.Search("s1", new float[] { 0f, 0f }, topK: 1);

        act.Should().NotThrow();
    }

    [Fact]
    public void Search_HandlesUnequalEmbeddingDimensions_Gracefully()
    {
        _sut.AddChunks(new[] { MakeChunk("s1", "f1", "text", new float[] { 1f, 0f, 0f }) });

        // Query with different dimension — cosine similarity should return 0 (no match)
        var act = () => _sut.Search("s1", new float[] { 1f, 0f }, topK: 1);

        act.Should().NotThrow();
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static DocumentChunk MakeChunk(
        string sessionId, string fileId, string text, float[] embedding) => new()
    {
        SessionId = sessionId,
        FileId = fileId,
        FileName = $"{fileId}.pdf",
        Text = text,
        Embedding = embedding
    };
}
