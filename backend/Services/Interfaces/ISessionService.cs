using SafeQueryAI.Api.Models;

namespace SafeQueryAI.Api.Services.Interfaces;

public interface ISessionService
{
    /// <summary>Creates a new session and returns it.</summary>
    SessionInfo CreateSession();

    /// <summary>Retrieves an existing session, or null if not found.</summary>
    SessionInfo? GetSession(string sessionId);

    /// <summary>Adds a file entry to the session.</summary>
    void AddFileToSession(string sessionId, StoredFileInfo file);

    /// <summary>Removes all files and data for the session.</summary>
    bool ClearSession(string sessionId);

    /// <summary>
    /// Returns the IDs of all sessions whose last-accessed time is older than <paramref name="timeout"/>.
    /// Used by the background expiry service to enforce the temporary-session promise.
    /// </summary>
    IReadOnlyList<string> GetExpiredSessionIds(TimeSpan timeout);
}
