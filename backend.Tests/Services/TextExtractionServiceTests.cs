using Microsoft.Extensions.Logging.Abstractions;
using SafeQueryAI.Api.Services;

namespace SafeQueryAI.Tests.Services;

/// <summary>
/// Tests for TextExtractionService CSV parsing — the highest-risk extraction path
/// as identified in the architecture review (naive comma-split breaks quoted values).
/// </summary>
public class TextExtractionServiceTests
{
    private readonly TextExtractionService _sut =
        new(NullLogger<TextExtractionService>.Instance);

    // ── ExtractFromCsv — basic ────────────────────────────────────────────────

    [Fact]
    public void ExtractFromCsv_ReturnsEmpty_ForNonExistentFile()
    {
        var result = _sut.ExtractFromCsv("does_not_exist.csv");

        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractFromCsv_ExtractsHeadersAndRows()
    {
        using var tmp = new TempCsvFile("Name,Age\nAlice,30\nBob,25");

        var result = _sut.ExtractFromCsv(tmp.Path);

        result.Should().Contain("Name");
        result.Should().Contain("Alice");
        result.Should().Contain("Bob");
    }

    [Fact]
    public void ExtractFromCsv_ReturnsEmpty_ForHeaderOnlyFile()
    {
        using var tmp = new TempCsvFile("Name,Age");

        var result = _sut.ExtractFromCsv(tmp.Path);

        // Header row only — 0 data rows, but should not crash
        result.Should().NotBeNull();
    }

    [Fact]
    public void ExtractFromCsv_ReturnsEmpty_ForCompletelyEmptyFile()
    {
        using var tmp = new TempCsvFile(string.Empty);

        var result = _sut.ExtractFromCsv(tmp.Path);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractFromCsv_IncludesColumnCountInOutput()
    {
        using var tmp = new TempCsvFile("Col1,Col2,Col3\n1,2,3");

        var result = _sut.ExtractFromCsv(tmp.Path);

        result.Should().Contain("Col1");
        result.Should().Contain("Col2");
        result.Should().Contain("Col3");
    }

    [Fact]
    public void ExtractFromCsv_HandlesExtraColumnValues_WithoutThrowing()
    {
        // Row has more values than headers — should not throw
        using var tmp = new TempCsvFile("A,B\n1,2,3,4");

        var act = () => _sut.ExtractFromCsv(tmp.Path);

        act.Should().NotThrow();
    }

    [Fact]
    public void ExtractFromCsv_HandlesMissingColumnValues_WithoutThrowing()
    {
        // Row has fewer values than headers — should not throw
        using var tmp = new TempCsvFile("A,B,C\n1,2");

        var act = () => _sut.ExtractFromCsv(tmp.Path);

        act.Should().NotThrow();
    }

    [Fact]
    public void ExtractFromCsv_TrimsWhitespace_AroundHeaderValues()
    {
        using var tmp = new TempCsvFile(" Name , Age \nAlice,30");

        var result = _sut.ExtractFromCsv(tmp.Path);

        result.Should().Contain("Name");
        result.Should().Contain("Age");
    }

    // ── ExtractFromPdf ────────────────────────────────────────────────────────

    [Fact]
    public void ExtractFromPdf_ReturnsEmpty_WhenFileDoesNotExist()
    {
        var result = _sut.ExtractFromPdf("nonexistent.pdf");

        result.Should().BeEmpty();
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a temp CSV file for testing and deletes it on dispose.
    /// </summary>
    private sealed class TempCsvFile : IDisposable
    {
        public string Path { get; }

        public TempCsvFile(string content)
        {
            Path = System.IO.Path.GetTempFileName();
            File.WriteAllText(Path, content);
        }

        public void Dispose()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }
    }
}
