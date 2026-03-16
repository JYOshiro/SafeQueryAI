namespace PrivateDoc.Api.Contracts;

public record AskQuestionResponse(
    string Question,
    string Answer,
    bool HasConfidentAnswer,
    List<EvidenceItem> Evidence
);

public record EvidenceItem(
    string FileName,
    string Snippet
);
