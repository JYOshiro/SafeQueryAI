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
}
