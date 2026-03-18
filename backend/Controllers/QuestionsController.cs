using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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

    /// <summary>
    /// Streams the answer token-by-token as Server-Sent Events (text/event-stream).
    /// Each SSE message is a JSON object with either <c>{"type":"token","content":"..."}</c>
    /// or a final <c>{"type":"done","question":"...","hasConfidentAnswer":bool,"evidence":[...]}</c> event.
    /// </summary>
    [HttpPost("stream")]
    public async Task AskQuestionStream(
        string sessionId,
        [FromBody] AskQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var session = _sessionService.GetSession(sessionId);
        if (session is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (string.IsNullOrWhiteSpace(request?.Question))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Append("X-Accel-Buffering", "no");

        await foreach (var chunk in _questionAnswering.StreamAnswerAsync(
            request.Question, sessionId, session.Files, cancellationToken))
        {
            string eventData;
            if (chunk.Token is not null)
            {
                eventData = JsonSerializer.Serialize(new { type = "token", content = chunk.Token });
            }
            else if (chunk.Final is not null)
            {
                var f = chunk.Final;
                eventData = JsonSerializer.Serialize(new
                {
                    type = "done",
                    question = f.Question,
                    hasConfidentAnswer = f.HasConfidentAnswer,
                    evidence = f.Evidence
                });
            }
            else continue;

            await Response.WriteAsync($"data: {eventData}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
