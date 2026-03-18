using Microsoft.Extensions.Logging.Abstractions;
using SafeQueryAI.Api.Services;
using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Tests.Services;

public class DocumentIndexingServiceTests
{
    private readonly Mock<IOllamaService> _ollama = new();
    private readonly Mock<IVectorStoreService> _vectorStore = new();
    private readonly DocumentIndexingService _sut;

    public DocumentIndexingServiceTests()
    {
        _sut = new DocumentIndexingService(
            _ollama.Object,
            _vectorStore.Object,
            NullLogger<DocumentIndexingService>.Instance);
    }

    // ── IndexFileAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task IndexFileAsync_CallsEmbedOnce_ForEachChunk()
    {
        // Short text should produce exactly 1 chunk
        var text = "This is a short document.";
        _ollama.Setup(o => o.GetEmbeddingAsync(It.IsAny<string>(), default))
               .ReturnsAsync(new float[] { 1f, 0f });

        await _sut.IndexFileAsync("s1", "f1", "doc.pdf", text);

        _ollama.Verify(o => o.GetEmbeddingAsync(It.IsAny<string>(), default), Times.AtLeastOnce);
    }

    [Fact]
    public async Task IndexFileAsync_AddsChunksToVectorStore()
    {
        var text = "This is a short document.";
        _ollama.Setup(o => o.GetEmbeddingAsync(It.IsAny<string>(), default))
               .ReturnsAsync(new float[] { 1f, 0f });

        await _sut.IndexFileAsync("s1", "f1", "doc.pdf", text);

        _vectorStore.Verify(v => v.AddChunks(It.IsAny<IEnumerable<Api.Models.DocumentChunk>>()), Times.Once);
    }

    [Fact]
    public async Task IndexFileAsync_ProducesMultipleChunks_ForLongDocument()
    {
        // Generate text clearly exceeding the 600-char chunk size
        var text = string.Join(" ", Enumerable.Repeat("word", 500));
        int chunkCount = 0;

        _ollama.Setup(o => o.GetEmbeddingAsync(It.IsAny<string>(), default))
               .ReturnsAsync(new float[] { 1f });
        _vectorStore.Setup(v => v.AddChunks(It.IsAny<IEnumerable<Api.Models.DocumentChunk>>()))
                    .Callback<IEnumerable<Api.Models.DocumentChunk>>(chunks => chunkCount = chunks.Count());

        await _sut.IndexFileAsync("s1", "f1", "doc.pdf", text);

        chunkCount.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task IndexFileAsync_DoesNotCallEmbed_WhenTextIsEmpty()
    {
        await _sut.IndexFileAsync("s1", "f1", "empty.pdf", string.Empty);

        // Empty text produces no chunks, so embed is never called
        _ollama.Verify(o => o.GetEmbeddingAsync(It.IsAny<string>(), default), Times.Never);
        // AddChunks may be called with an empty collection — verify no chunks were actually stored
        _vectorStore.Verify(
            v => v.AddChunks(It.Is<IEnumerable<Api.Models.DocumentChunk>>(c => c.Any())),
            Times.Never);
    }

    [Fact]
    public async Task IndexFileAsync_ThrowsOnCancellation()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _ollama.Setup(o => o.GetEmbeddingAsync(It.IsAny<string>(), cts.Token))
               .ThrowsAsync(new OperationCanceledException());

        // Text must be non-empty so chunking actually runs
        var text = string.Join(" ", Enumerable.Repeat("word", 200));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.IndexFileAsync("s1", "f1", "doc.pdf", text, cts.Token));
    }

    // ── RemoveFileIndex / RemoveSessionIndex ──────────────────────────────────

    [Fact]
    public void RemoveFileIndex_DelegatesToVectorStore()
    {
        _sut.RemoveFileIndex("s1", "f1");

        _vectorStore.Verify(v => v.RemoveFile("s1", "f1"), Times.Once);
    }

    [Fact]
    public void RemoveSessionIndex_DelegatesToVectorStore()
    {
        _sut.RemoveSessionIndex("s1");

        _vectorStore.Verify(v => v.RemoveSession("s1"), Times.Once);
    }
}
