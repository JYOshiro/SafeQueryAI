namespace SafeQueryAI.Api.Contracts;

public record FileUploadResponse(
    string FileId,
    string FileName,
    string FileType,
    long FileSizeBytes,
    DateTime UploadedAt
);
