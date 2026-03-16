using Microsoft.AspNetCore.Mvc;
using SafeQueryAI.Api.Contracts;
using SafeQueryAI.Api.Services.Interfaces;

namespace SafeQueryAI.Api.Controllers;

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
    /// Uses RAG (Ollama) when available, otherwise falls back to keyword matching.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AskQuestion(
        string sessionId,
        [FromBody] AskQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var session = _sessionService.GetSession(sessionId);
        if (session is null)
            return NotFound(new ErrorResponse("Session not found"));

        if (string.IsNullOrWhiteSpace(request?.Question))
            return BadRequest(new ErrorResponse("Question is required"));

        var response = await _questionAnswering.AnswerAsync(
            request.Question, sessionId, session.Files, cancellationToken);

        return Ok(response);
    }
}
