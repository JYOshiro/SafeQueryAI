using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SafeQueryAI.Api.Services;

namespace SafeQueryAI.Tests.Services;

public class FileStorageServiceTests : IDisposable
{
    private readonly string _basePath;
    private readonly FileStorageService _sut;

    public FileStorageServiceTests()
    {
        // Each test gets its own isolated temp directory
        _basePath = Path.Combine(Path.GetTempPath(), $"SafeQueryAI_Test_{Guid.NewGuid():N}");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SafeQueryAI:TempStoragePath"] = _basePath
            })
            .Build();

        // Create dir before constructing so startup purge does not fail
        Directory.CreateDirectory(_basePath);
        _sut = new FileStorageService(config, NullLogger<FileStorageService>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_basePath))
            Directory.Delete(_basePath, recursive: true);
    }

    // ── SaveFileAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveFileAsync_CreatesFileOnDisk()
    {
        var formFile = MakeFormFile("hello world", "test.pdf");
        var fileId = Guid.NewGuid().ToString("N");

        var path = await _sut.SaveFileAsync("session1", fileId, formFile);

        File.Exists(path).Should().BeTrue();
    }

    [Fact]
    public async Task SaveFileAsync_UsesFileIdAsFileName()
    {
        var fileId = Guid.NewGuid().ToString("N");
        var formFile = MakeFormFile("content", "original.pdf");

        var path = await _sut.SaveFileAsync("session1", fileId, formFile);

        Path.GetFileNameWithoutExtension(path).Should().Be(fileId);
    }

    [Fact]
    public async Task SaveFileAsync_PreservesFileExtension()
    {
        var fileId = "abc123";
        var formFile = MakeFormFile("a,b,c\n1,2,3", "data.csv");

        var path = await _sut.SaveFileAsync("session1", fileId, formFile);

        Path.GetExtension(path).Should().Be(".csv");
    }

    [Fact]
    public async Task SaveFileAsync_ScopesFilesToSessionDirectory()
    {
        var formFile = MakeFormFile("content", "test.pdf");
        var path = await _sut.SaveFileAsync("session_abc", Guid.NewGuid().ToString("N"), formFile);

        path.Should().Contain("session_abc");
    }

    // ── DeleteSessionFiles ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteSessionFiles_RemovesSessionDirectory()
    {
        var formFile = MakeFormFile("content", "test.pdf");
        await _sut.SaveFileAsync("session2", Guid.NewGuid().ToString("N"), formFile);

        _sut.DeleteSessionFiles("session2");

        var sessionDir = Path.Combine(_basePath, "session2");
        Directory.Exists(sessionDir).Should().BeFalse();
    }

    [Fact]
    public void DeleteSessionFiles_DoesNotThrow_WhenSessionDirDoesNotExist()
    {
        var act = () => _sut.DeleteSessionFiles("nonexistent_session");

        act.Should().NotThrow();
    }

    [Fact]
    public async Task DeleteSessionFiles_DoesNotDeleteOtherSessionDirectories()
    {
        var f1 = MakeFormFile("a", "a.pdf");
        var f2 = MakeFormFile("b", "b.pdf");
        await _sut.SaveFileAsync("sessionA", Guid.NewGuid().ToString("N"), f1);
        await _sut.SaveFileAsync("sessionB", Guid.NewGuid().ToString("N"), f2);

        _sut.DeleteSessionFiles("sessionA");

        Directory.Exists(Path.Combine(_basePath, "sessionB")).Should().BeTrue();
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static IFormFile MakeFormFile(string content, string fileName)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.Length).Returns(bytes.Length);
        mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
            .Callback<Stream, CancellationToken>((dest, _) => stream.CopyTo(dest))
            .Returns(Task.CompletedTask);

        return mock.Object;
    }
}
