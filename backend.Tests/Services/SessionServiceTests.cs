using SafeQueryAI.Api.Models;
using SafeQueryAI.Api.Services;

namespace SafeQueryAI.Tests.Services;

public class SessionServiceTests
{
    private readonly SessionService _sut = new();

    // ── CreateSession ─────────────────────────────────────────────────────────

    [Fact]
    public void CreateSession_ReturnsSessionWithNonEmptyId()
    {
        var session = _sut.CreateSession();

        session.SessionId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CreateSession_ReturnsDifferentIdsForEachCall()
    {
        var a = _sut.CreateSession();
        var b = _sut.CreateSession();

        a.SessionId.Should().NotBe(b.SessionId);
    }

    [Fact]
    public void CreateSession_SetsCreatedAtAndLastAccessedAt_ToUtcNow()
    {
        var before = DateTime.UtcNow;
        var session = _sut.CreateSession();
        var after = DateTime.UtcNow;

        session.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        session.LastAccessedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    // ── GetSession ────────────────────────────────────────────────────────────

    [Fact]
    public void GetSession_ReturnsNull_WhenSessionDoesNotExist()
    {
        var result = _sut.GetSession("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public void GetSession_ReturnsSameSession_WhenSessionExists()
    {
        var created = _sut.CreateSession();

        var retrieved = _sut.GetSession(created.SessionId);

        retrieved.Should().NotBeNull();
        retrieved!.SessionId.Should().Be(created.SessionId);
    }

    [Fact]
    public void GetSession_UpdatesLastAccessedAt()
    {
        var session = _sut.CreateSession();
        var originalAccess = session.LastAccessedAt;

        // small pause to ensure measurable time difference
        Thread.Sleep(5);
        _sut.GetSession(session.SessionId);

        session.LastAccessedAt.Should().BeAfter(originalAccess);
    }

    // ── AddFileToSession ──────────────────────────────────────────────────────

    [Fact]
    public void AddFileToSession_AddsFileToExistingSession()
    {
        var session = _sut.CreateSession();
        var file = MakeFile("file1");

        _sut.AddFileToSession(session.SessionId, file);

        var retrieved = _sut.GetSession(session.SessionId);
        retrieved!.Files.Should().ContainSingle(f => f.FileId == "file1");
    }

    [Fact]
    public void AddFileToSession_DoesNotThrow_WhenSessionNotFound()
    {
        var act = () => _sut.AddFileToSession("bad-id", MakeFile("x"));

        act.Should().NotThrow();
    }

    // ── ClearSession ──────────────────────────────────────────────────────────

    [Fact]
    public void ClearSession_ReturnsFalse_WhenSessionNotFound()
    {
        var result = _sut.ClearSession("nonexistent");

        result.Should().BeFalse();
    }

    [Fact]
    public void ClearSession_ReturnsTrue_AndRemovesSession()
    {
        var session = _sut.CreateSession();

        var result = _sut.ClearSession(session.SessionId);

        result.Should().BeTrue();
        _sut.GetSession(session.SessionId).Should().BeNull();
    }

    // ── GetExpiredSessionIds ──────────────────────────────────────────────────

    [Fact]
    public void GetExpiredSessionIds_ReturnsExpiredSessions()
    {
        var session = _sut.CreateSession();
        // Force last-accessed into the past by accessing internal state.
        // Use a very short timeout so the created session appears expired.
        var expiredIds = _sut.GetExpiredSessionIds(TimeSpan.FromMilliseconds(-1));

        expiredIds.Should().Contain(session.SessionId);
    }

    [Fact]
    public void GetExpiredSessionIds_DoesNotReturnActiveSessions()
    {
        _sut.CreateSession();

        var expiredIds = _sut.GetExpiredSessionIds(TimeSpan.FromHours(1));

        expiredIds.Should().BeEmpty();
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static StoredFileInfo MakeFile(string id) => new()
    {
        FileId = id,
        OriginalFileName = $"{id}.pdf",
        FileType = "pdf",
        FileSizeBytes = 100,
        ExtractedText = "sample content"
    };
}
