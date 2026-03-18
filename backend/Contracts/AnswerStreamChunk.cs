namespace SafeQueryAI.Api.Contracts;

/// <summary>
/// A single event in the token-streaming answer pipeline.
/// Either contains a partial answer token from the LLM, or the final metadata once the stream ends.
/// Exactly one of <see cref="Token"/> or <see cref="Final"/> will be non-null for each chunk.
/// </summary>
/// <param name="Token">A partial answer text returned by the LLM, or <c>null</c>.</param>
/// <param name="Final">Metadata (confidence, evidence) emitted once streaming is complete, or <c>null</c>.</param>
public record AnswerStreamChunk(string? Token, AskQuestionResponse? Final);
