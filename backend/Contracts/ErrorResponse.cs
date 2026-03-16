namespace SafeQueryAI.Api.Contracts;

public record ErrorResponse(
    string Error,
    string? Detail = null
);
