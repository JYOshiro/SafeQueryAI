namespace PrivateDoc.Api.Contracts;

public record ClearSessionResponse(
    string SessionId,
    bool Cleared,
    string Message
);
