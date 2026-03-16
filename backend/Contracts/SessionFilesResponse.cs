namespace PrivateDoc.Api.Contracts;

public record SessionFilesResponse(
    string SessionId,
    List<SessionFileItem> Files
);

public record SessionFileItem(
    string FileId,
    string FileName,
    string FileType,
    long FileSizeBytes,
    DateTime UploadedAt
);
