using Microsoft.AspNetCore.Mvc;
using SafeQueryAI.Api.Contracts;
using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Api.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    /// <summary>
    /// Creates a new temporary session. The session ID is used in all subsequent requests.
    /// </summary>
    [HttpPost]
    public IActionResult CreateSession()
    {
        var session = _sessionService.CreateSession();
        return Ok(new CreateSessionResponse(session.SessionId, session.CreatedAt));
    }

    /// <summary>
    /// Retrieves session metadata (does not return file content).
    /// </summary>
    [HttpGet("{sessionId}")]
    public IActionResult GetSession(string sessionId)
    {
        var session = _sessionService.GetSession(sessionId);
        if (session is null)
            return NotFound(new ErrorResponse("Session not found", $"No session exists with ID: {sessionId}"));

        return Ok(new CreateSessionResponse(session.SessionId, session.CreatedAt));
    }

    /// <summary>
    /// Clears all uploaded files and removes the session from memory.
    /// </summary>
    [HttpDelete("{sessionId}")]
    public IActionResult ClearSession(
        string sessionId,
        [FromServices] IFileStorageService fileStorage)
    {
        var session = _sessionService.GetSession(sessionId);
        if (session is null)
            return NotFound(new ErrorResponse("Session not found", $"No session exists with ID: {sessionId}"));

        fileStorage.DeleteSessionFiles(sessionId);
        _sessionService.ClearSession(sessionId);

        return Ok(new ClearSessionResponse(sessionId, true, "Session cleared. All uploaded files have been removed."));
    }
}
