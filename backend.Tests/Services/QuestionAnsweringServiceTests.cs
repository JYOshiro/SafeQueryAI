using Microsoft.Extensions.Logging.Abstractions;
using SafeQueryAI.Api.Contracts;
using SafeQueryAI.Api.Models;
using SafeQueryAI.Api.Services;
using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Tests.Services;

public class QuestionAnsweringServiceTests
{
    private readonly Mock<IOllamaService> _ollama = new();
    private readonly Mock<IVectorStoreService> _vectorStore = new();
    private readonly QuestionAnsweringService _sut;

    public QuestionAnsweringServiceTests()
    {
        _sut = new QuestionAnsweringService(
            _ollama.Object,
            _vectorStore.Object,
            NullLogger<QuestionAnsweringService>.Instance);
    }

    // ── No files in session ───────────────────────────────────────────────────

    [Fact]
    public async Task AnswerAsync_ReturnsNotConfident_WhenNoFilesInSession()
    {
        var result = await _sut.AnswerAsync("question", "s1", Array.Empty<StoredFileInfo>());

        result.HasConfidentAnswer.Should().BeFalse();
        result.Answer.Should().Contain("No files");
    }

    // ── RAG path ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AnswerAsync_UsesRagPath_WhenChunksFound()
    {
        var files = new[] { MakeFile("f1", "some content about cats") };
        var embedding = new float[] { 1f, 0f };
        var chunks = new[] { MakeChunk("s1", "f1", "cats are great") };

        _ollama.Setup(o => o.GetEmbeddingAsync("question", default)).ReturnsAsync(embedding);
        _vectorStore.Setup(v => v.Search("s1", embedding, 5)).Returns(chunks);
        _ollama.Setup(o => o.GenerateAnswerAsync("question", It.IsAny<IEnumerable<string>>(), default))
               .ReturnsAsync("Cats are great animals.");

        var result = await _sut.AnswerAsync("question", "s1", files);

        result.HasConfidentAnswer.Should().BeTrue();
        result.Answer.Should().Be("Cats are great animals.");
        result.Evidence.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AnswerAsync_IncludesEvidenceFromChunks()
    {
        var files = new[] { MakeFile("f1", "content") };
        var embedding = new float[] { 1f, 0f };
        var chunks = new[]
        {
            MakeChunk("s1", "f1", "chunk about dogs"),
            MakeChunk("s1", "f1", "chunk about cats"),
        };

        _ollama.Setup(o => o.GetEmbeddingAsync(It.IsAny<string>(), default)).ReturnsAsync(embedding);
        _vectorStore.Setup(v => v.Search("s1", embedding, 5)).Returns(chunks);
        _ollama.Setup(o => o.GenerateAnswerAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), default))
               .ReturnsAsync("Answer.");

        var result = await _sut.AnswerAsync("question", "s1", files);

        result.Evidence.Should().ContainSingle(e => e.FileName == "f1.pdf");
    }

    // ── Keyword fallback when Ollama unavailable ──────────────────────────────

    [Fact]
    public async Task AnswerAsync_FallsBackToKeyword_WhenOllamaThrowsHttpException()
    {
        var files = new[] { MakeFile("f1", "the document discusses machine learning algorithms") };

        _ollama.Setup(o => o.GetEmbeddingAsync(It.IsAny<string>(), default))
               .ThrowsAsync(new HttpRequestException("Ollama offline"));

        var result = await _sut.AnswerAsync("machine learning", "s1", files);

        // Should not throw; keyword fallback should produce an answer
        result.Should().NotBeNull();
        result.Answer.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AnswerAsync_FallsBackToKeyword_WhenVectorStoreReturnsNoChunks()
    {
        var files = new[] { MakeFile("f1", "the document discusses machine learning algorithms") };
        var embedding = new float[] { 1f, 0f };

        _ollama.Setup(o => o.GetEmbeddingAsync(It.IsAny<string>(), default)).ReturnsAsync(embedding);
        _vectorStore.Setup(v => v.Search("s1", embedding, 5)).Returns(Array.Empty<DocumentChunk>());

        var result = await _sut.AnswerAsync("machine learning", "s1", files);

        result.Should().NotBeNull();
    }

    // ── Keyword fallback confidence ───────────────────────────────────────────

    [Fact]
    public async Task AnswerAsync_ReturnsNotConfident_WhenKeywordsNotFoundInFiles()
    {
        var files = new[] { MakeFile("f1", "nothing relevant here at all xyz") };

        _ollama.Setup(o => o.GetEmbeddingAsync(It.IsAny<string>(), default))
               .ThrowsAsync(new HttpRequestException());

        var result = await _sut.AnswerAsync("zzzzz unique term nowhere", "s1", files);

        result.HasConfidentAnswer.Should().BeFalse();
    }

    // ── Streaming path ────────────────────────────────────────────────────────

    [Fact]
    public async Task StreamAnswerAsync_YieldsFinalChunk_WhenNoFilesUploaded()
    {
        var chunks = new List<AnswerStreamChunk>();

        await foreach (var c in _sut.StreamAnswerAsync("q", "s1", Array.Empty<StoredFileInfo>()))
            chunks.Add(c);

        chunks.Should().ContainSingle();
        chunks[0].Final.Should().NotBeNull();
        chunks[0].Final!.HasConfidentAnswer.Should().BeFalse();
    }

    [Fact]
    public async Task StreamAnswerAsync_YieldsTokensThenFinalChunk_WhenRagSucceeds()
    {
        var files = new[] { MakeFile("f1", "some content") };
        var embedding = new float[] { 1f, 0f };
        var chunks = new[] { MakeChunk("s1", "f1", "some content") };

        _ollama.Setup(o => o.GetEmbeddingAsync(It.IsAny<string>(), default)).ReturnsAsync(embedding);
        _vectorStore.Setup(v => v.Search("s1", embedding, 5)).Returns(chunks);
        _ollama.Setup(o => o.GenerateAnswerStreamAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), default))
               .Returns(AsyncTokens("Hello", " world"));

        var received = new List<AnswerStreamChunk>();
        await foreach (var c in _sut.StreamAnswerAsync("question", "s1", files))
            received.Add(c);

        received.Where(c => c.Token != null).Should().HaveCount(2);
        received.Last().Final.Should().NotBeNull();
        received.Last().Final!.HasConfidentAnswer.Should().BeTrue();
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static async IAsyncEnumerable<string> AsyncTokens(params string[] tokens)
    {
        foreach (var t in tokens) yield return t;
        await Task.CompletedTask;
    }

    private static StoredFileInfo MakeFile(string id, string text) => new()
    {
        FileId = id,
        OriginalFileName = $"{id}.pdf",
        FileType = "pdf",
        FileSizeBytes = text.Length,
        ExtractedText = text
    };

    private static DocumentChunk MakeChunk(string sessionId, string fileId, string text) => new()
    {
        SessionId = sessionId,
        FileId = fileId,
        FileName = $"{fileId}.pdf",
        Text = text,
        Embedding = new float[] { 1f, 0f }
    };
}
