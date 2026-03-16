namespace SafeQueryAI.Api.Models;

/// <summary>
/// Metadata for a file that has been uploaded to a session.
/// The actual file is stored in a temporary local folder scoped to the session.
/// </summary>
public class StoredFileInfo
{
    public string FileId { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string FileType { get; init; } = string.Empty;   // "pdf" or "csv"
    public long FileSizeBytes { get; init; }
    public DateTime UploadedAt { get; init; } = DateTime.UtcNow;
    public string LocalPath { get; init; } = string.Empty;

    /// <summary>
    /// Extracted plain-text content used for answering questions.
    /// Not returned to the client; kept server-side only.
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;
}
