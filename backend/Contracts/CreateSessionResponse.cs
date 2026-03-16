namespace SafeQueryAI.Api.Contracts;

public record CreateSessionResponse(
    string SessionId,
    DateTime CreatedAt
);
