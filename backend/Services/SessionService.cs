using System.Collections.Concurrent;
using SafeQueryAI.Api.Models;
using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Api.Services;

/// <summary>
/// Manages in-memory session state. No data is persisted to a database.
/// Sessions exist only for the lifetime of the server process or until explicitly cleared.
/// </summary>
public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();

    public SessionInfo CreateSession()
    {
        var session = new SessionInfo
        {
            SessionId = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };

        _sessions[session.SessionId] = session;
        return session;
    }

    public SessionInfo? GetSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.LastAccessedAt = DateTime.UtcNow;
            return session;
        }
        return null;
    }

    public void AddFileToSession(string sessionId, StoredFileInfo file)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.Files.Add(file);
            session.LastAccessedAt = DateTime.UtcNow;
        }
    }

    public bool ClearSession(string sessionId)
    {
        return _sessions.TryRemove(sessionId, out _);
    }

    public IReadOnlyList<string> GetExpiredSessionIds(TimeSpan timeout)
    {
        var cutoff = DateTime.UtcNow - timeout;
        return _sessions.Values
            .Where(s => s.LastAccessedAt < cutoff)
            .Select(s => s.SessionId)
            .ToList();
    }
}
