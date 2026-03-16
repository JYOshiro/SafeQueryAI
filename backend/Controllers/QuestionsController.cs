using Microsoft.AspNetCore.Mvc;
using PrivateDoc.Api.Contracts;
using PrivateDoc.Api.Services.Interfaces;

namespace PrivateDoc.Api.Controllers;

[ApiController]
[Route("api/sessions/{sessionId}/questions")]
public class QuestionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IQuestionAnsweringService _questionAnswering;

    public QuestionsController(
        ISessionService sessionService,
        IQuestionAnsweringService questionAnswering)
    {
        _sessionService = sessionService;
        _questionAnswering = questionAnswering;
    }

    /// <summary>
    /// Asks a question and receives an answer grounded in the session's uploaded files only.
    /// </summary>
    [HttpPost]
    public IActionResult AskQuestion(string sessionId, [FromBody] AskQuestionRequest request)
    {
        var session = _sessionService.GetSession(sessionId);
        if (session is null)
            return NotFound(new ErrorResponse("Session not found"));

        if (string.IsNullOrWhiteSpace(request?.Question))
            return BadRequest(new ErrorResponse("Question is required"));

        var response = _questionAnswering.Answer(request.Question, session.Files);
        return Ok(response);
    }
}
